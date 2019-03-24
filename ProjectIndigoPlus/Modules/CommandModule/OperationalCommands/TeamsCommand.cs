using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using MySql.Data.MySqlClient;
using ProjectIndigoPlus.Modules.HelperModule;
using ProjectIndigoPlus.Modules.ModelModule;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectIndigoPlus.Modules.Commands
{
    internal class Teams
    {
        [Command("team"),
         Aliases(new string[] { "lookup" }),
         Description("Lookup a player's team. Only works when the tourney has started.")]
        public async Task TeamCommand(CommandContext context, [Description("The user -")] DiscordUser input = null)
        {
            MySqlConnection conn = DatabaseHelper.GetClosedConnection();
            TourneyModel tourney = await DatabaseHelper.GetLatestTourneyAsync(conn);
            if (tourney.RegEnd <= DateTimeOffset.Now)
            {
                DiscordUser user = input;
                if (input == null)
                {
                    user = context.User;
                }

                Dictionary<string, string> team = await conn.GetRowDataFromDBAsync<string>(
                    $"SELECT `team` FROM `teams` LEFT JOIN `members` ON `teams`.`uid` = `members`.`uid` WHERE `tid` = {tourney.Tid} AND `members`.`uid` = {user.Id}");

                if(team == null)
                {
                    await context.RespondAsync("", false, new DiscordEmbedBuilder()
                    {
                        Color = DiscordColor.Red,
                        Title = $"User \"{user.Username}\" not found",
                        Description = "It seems that that user isn't participating in this tourney."
                    });
                }
                else
                {
                    await context.RespondAsync("", false, new DiscordEmbedBuilder()
                    {
                        Color = Bot._config.Color,
                        Title = $"{user.Username}'s Team:",
                        Description = $"```{team["team"]}```"
                    });
                }
            }
            else
            {
                await context.RespondAsync("", false, new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Red,
                    Title = "Signups still open",
                    Description = "While the signups are open you can't check other players teams."
                });
            }
        }
    }
}
