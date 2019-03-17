using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using MySql.Data.MySqlClient;
using ProjectIndigoPlus.Entities;
using ProjectIndigoPlus.Modules.HelperModule;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ProjectIndigoPlus.Modules.Commands
{
    internal class BattleReplay
    {
        private const string SQLSTRING = "SELECT `battles`.`round` as Round, `battles`.`player1` as P1ID, `battles`.`player2` as P2ID, P1.showdownusername as P1name, P2.showdownusername as P2name FROM `battles` INNER JOIN `members` AS P1 ON ( P1.`uid` = `battles`.`player1` ) INNER JOIN `members` AS P2 ON ( P2.`uid` = `battles`.`player2` ) WHERE (`player1` = @uid OR `player2` = @uid) AND `battles`.`tid` = @tid";

        private readonly Dependencies dep;
        public BattleReplay(Dependencies d)
        {
            dep = d;
        }

        [Command("submitreplay"),
            Aliases(new[] { "replay" }),
            Description("Submit the battle replay for your current battle.")]
        public async Task Command(CommandContext context, string input)
        {
            if (!Regex.IsMatch(input, @"(https?:\/\/replay\.pokemonshowdown\.com\/[a-z\d-]+$)"))
            {
                await context.Channel.SendMessageAsync("", false, new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Red,
                    Title = "Invalid URL",
                    Description = $"{context.User.Mention} The URL you provided is invalid, please send me the replay URL.\n\nYour input: `{input}`"
                });
                return;
            }
            MySqlConnection conn = DatabaseHelper.GetClosedConnection();
            try
            {
                await conn.OpenAsync();

                MySqlCommand cmd = new MySqlCommand(SQLSTRING, conn);
                cmd.Parameters.Add("uid", MySqlDbType.UInt64).Value = context.User.Id;
                cmd.Parameters.Add("tid", MySqlDbType.Int32).Value = (await conn.GetLatestTourneyAsync()).Tid;

                using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        
                    }
                }
            }
            catch (Exception e)
            {
                Bot.DebugLogger.LogMessage(DSharpPlus.LogLevel.Critical, "Battle Command" + "", e.ToString(), DateTime.Now);
            }
            finally
            {
                await conn.CloseAsync();
            }

            #region SubmitURL
            DiscordMessage cfrMessage = await context.Channel.SendMessageAsync("", false, new DiscordEmbedBuilder()
            {
                Color = Bot._config.Color,
                Title = "Battle Replay Submission Confirmation",
                Description = $"{context.User.Mention} is the following URL correct?\n\n{input}"
            });
            await cfrMessage.CreateReactionAsync(DiscordEmoji.FromName(context.Client, ":x:"));
            await cfrMessage.CreateReactionAsync(DiscordEmoji.FromName(context.Client, ":white_check_mark:"));

            ReactionContext react = await dep.Interactivity.WaitForMessageReactionAsync(x => x.GetDiscordName() == ":x:" || x.GetDiscordName() == ":white_check_mark:", cfrMessage, context.User);
            if (react == null)
            {
                await context.Channel.SendMessageAsync("", false, new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Red,
                    Title = "Battle Replay Submission",
                    Description = $"It appears I lost you :cry:\n To restart type `{Bot._config.Prefix + context.Command.Name}`"
                });
                return;
            }
            else if (react.Emoji.GetDiscordName() == ":x:")
            {
                await context.Channel.SendMessageAsync("", false, new DiscordEmbedBuilder()
                {
                    Color = Bot._config.Color,
                    Title = "Battle Replay Submission",
                    Description = "Please call the command again if you wish to retry."
                });
            }
            else if (react.Emoji.GetDiscordName() == ":white_check_mark:")
            {

            }
            #endregion
        }
    }
}