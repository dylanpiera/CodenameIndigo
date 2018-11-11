using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using MySql.Data.MySqlClient;
using ProjectIndigoPlus.Entities;
using ProjectIndigoPlus.Modules.HelperModule;
using ProjectIndigoPlus.Modules.ModelModule;
using System;
using System.Threading.Tasks;

namespace ProjectIndigoPlus.Modules.Commands
{
    internal class List
    {
        private readonly Dependencies dep;
        public List(Dependencies d)
        {
            dep = d;
        }

        [Command("playerlist"),
            Aliases(new[] { "list", "players" }),
            Description("Display all users in a given tournament.")]
        public async Task Command(CommandContext context, string input = "")
        {
            TourneyModel tourney = null;
            MySqlConnection conn = DatabaseHelper.GetClosedConnection();

            if (string.IsNullOrEmpty(input))
            {
                tourney = await conn.GetLatestTourneyAsync();
                if (tourney == null)
                {
                    await context.RespondAsync("", false, new DiscordEmbedBuilder()
                    {
                        Title = "Tournament not found.",
                        Description = $"An error occured. Please contact an administrator..",
                        Color = DiscordColor.DarkRed
                    });
                    return;
                }
            }
            else if (int.TryParse(input.Trim(), out int id))
            {
                (bool, TourneyModel) value = await conn.GetTourneyByIDAsync(id);
                if (value.Item1)
                {
                    tourney = value.Item2;
                }
                else
                {
                    await context.RespondAsync("", false, new DiscordEmbedBuilder()
                    {
                        Title = "Tournament not found.",
                        Description = $"I couldn't find tournament with id `{id}`.",
                        Color = DiscordColor.DarkRed
                    });
                    return;
                }
            }
            else
            {
                await context.RespondAsync("", false, new DiscordEmbedBuilder()
                {
                    Title = "Tournament not found.",
                    Description = $"I couldn't find tournament with that input. Please try `{Bot._config.Prefix}{context.Command.Name} (optional)[tourney id]`",
                    Color = DiscordColor.DarkRed
                });
                return;
            }

            DiscordEmbedBuilder players = new DiscordEmbedBuilder()
            {
                Title = $"Player List for the {tourney.Name}",
                Color = Bot._config.Color,
                Description = $"The participating discord player names are as followed:\n\n",
                Footer = new DiscordEmbedBuilder.EmbedFooter()
                {
                    Text = $"For more info regarding a user, see `{Bot._config.Prefix}help user` or `{Bot._config.Prefix}help team`"
                }
            };

            try
            {
                await conn.OpenAsync();

                MySqlCommand cmd = new MySqlCommand($"SELECT * FROM teams LEFT JOIN members ON teams.uid = members.uid WHERE tid = {tourney.Tid} ORDER BY regdate ASC LIMIT 0, {tourney.MaxPlayers}", conn);

                using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                {
                    int i = 0;
                    while (await reader.ReadAsync())
                    {
                        players.Description += $"{++i}. {reader.GetString("discordusername")}\n";
                    }
                }
            }
            catch (Exception e)
            {
                Bot.DebugLogger.LogMessage(DSharpPlus.LogLevel.Critical, "ListCommand", e.ToString(), DateTime.Now);
            }
            finally
            {
                await conn.CloseAsync();
            }
            await context.RespondAsync("", false, players.Build());
        }
    }
}
