using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using MySql.Data.MySqlClient;
using ProjectIndigoPlus.Entities;
using ProjectIndigoPlus.Modules.HelperModule;
using ProjectIndigoPlus.Modules.ModelModule;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ProjectIndigoPlus.Modules.Commands
{
    internal class BattleReplay
    {
        private const string SQLSTRING = "SELECT `battles`.`round` as Round, `battles`.`replay1` as Replay1, `battles`.`replay2` as Replay2, `battles`.`player1` as P1ID, `battles`.`player2` as P2ID, P1.showdownusername as P1name, P2.showdownusername as P2name FROM `battles` INNER JOIN `members` AS P1 ON ( P1.`uid` = `battles`.`player1` ) INNER JOIN `members` AS P2 ON ( P2.`uid` = `battles`.`player2` ) WHERE (`player1` = @uid OR `player2` = @uid) AND `battles`.`tid` = @tid ORDER BY `battles`.`round` DESC LIMIT 0,1";

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
            BattleModel battle = null;
            MySqlConnection conn = DatabaseHelper.GetClosedConnection();
            TourneyModel tourney = await conn.GetLatestTourneyAsync();
            try
            {
                await conn.OpenAsync();

                MySqlCommand cmd = new MySqlCommand(SQLSTRING, conn);
                cmd.Parameters.Add("uid", MySqlDbType.UInt64).Value = context.User.Id;
                cmd.Parameters.Add("tid", MySqlDbType.Int32).Value = tourney.Tid;

                using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        battle = new BattleModel()
                        {
                            Round = reader.GetInt32("Round"),
                            P1 = new Player(reader.GetUInt64("P1ID"), reader.GetString("P1name")),
                            P2 = new Player(reader.GetUInt64("P2ID"), reader.GetString("P2name")),
                            Replay1 = (await reader.IsDBNullAsync(reader.GetOrdinal("Replay1")) ? null : reader.GetString("Replay1")),
                            Replay2 = (await reader.IsDBNullAsync(reader.GetOrdinal("Replay2")) ? null : reader.GetString("Replay2"))
                        };
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

            if (battle == null)
            {
                await context.Channel.SendMessageAsync("", false, new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Red,
                    Title = "Battle not found",
                    Description = $"{context.User.Mention} are you sure you've signed up for the current tournament and aren't in a bye? :x:"
                });
                return;
            }

            int doesExist = 0;
            if (context.User.Id == battle.P1.Id && battle.Replay1 != null)
            {
                doesExist = 1;
            }
            else if (context.User.Id == battle.P2.Id && battle.Replay2 != null)
            {
                doesExist = 2;
            }
            if (doesExist != 0)
            {
                DiscordMessage overwriteCfrMessage = await context.Channel.SendMessageAsync("", false, new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Orange,
                    Title = $"Old Round {battle.Round} submission detected",
                    Description = $"{context.User.Mention} it seems you already provided me a replay: \n\n```{(doesExist == 1 ? battle.Replay1 : battle.Replay2)}```\n\nWould you like to submit a new one?"
                });
                await overwriteCfrMessage.CreateReactionAsync(DiscordEmoji.FromName(context.Client, ":x:"));
                await overwriteCfrMessage.CreateReactionAsync(DiscordEmoji.FromName(context.Client, ":white_check_mark:"));
                ReactionContext overwriteReact = await dep.Interactivity.WaitForMessageReactionAsync(x => x.GetDiscordName() == ":x:" || x.GetDiscordName() == ":white_check_mark:", overwriteCfrMessage, context.User);

                if (overwriteReact == null)
                {
                    await context.Channel.SendMessageAsync("", false, new DiscordEmbedBuilder()
                    {
                        Color = DiscordColor.Red,
                        Title = "Battle Replay Submission",
                        Description = $"It appears I lost you :cry:\n To restart type `{Bot._config.Prefix + context.Command.Name}`"
                    });
                    return;
                }
                else if (overwriteReact.Emoji.GetDiscordName() == ":x:")
                {
                    await context.Channel.SendMessageAsync("", false, new DiscordEmbedBuilder()
                    {
                        Color = Bot._config.Color,
                        Title = "Battle Replay Submission",
                        Description = "Please call the command again if you change your mind!"
                    });
                    return;
                }
                else if (overwriteReact.Emoji.GetDiscordName() == ":white_check_mark:")
                {
                    await context.Channel.SendMessageAsync("", false, new DiscordEmbedBuilder()
                    {
                        Color = DiscordColor.Orange,
                        Description = "Your old submission will be overwritten if you save the new replay."
                    });
                    await Task.Delay(150);
                }
            }

            int user = (context.User.Id == battle.P1.Id ? 1 : 2);

            if ((user == 1 ? battle.Replay2 : battle.Replay1) != null)
            {
                if ((user == 1 ? battle.Replay2 : battle.Replay1) != input)
                {
                    await context.Channel.SendMessageAsync("", false, new DiscordEmbedBuilder()
                    {
                        Color = DiscordColor.Orange,
                        Title = "URL Mismatch",
                        Description = $"The replay you're submissing differs from the one your opponent submitted. They submitted: \n\n" +
                        $"```{(user == 1 ? battle.Replay2 : battle.Replay1)}```\nYou submitted:\n" +
                        $"```{input}```"
                    });
                    await Task.Delay(150);
                }
            }

            #region SubmitURL
            DiscordMessage cfrMessage = await context.Channel.SendMessageAsync("", false, new DiscordEmbedBuilder()
            {
                Color = Bot._config.Color,
                Title = $"Round {battle.Round} Battle Replay Submission Confirmation",
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
                    Description = "Please call the command again if you change your mind!"
                });
                return;
            }
            else if (react.Emoji.GetDiscordName() == ":white_check_mark:")
            {
                try
                {
                    await conn.OpenAsync();

                    MySqlCommand cmd = new MySqlCommand($"UPDATE `battles` SET `{(user == 1 ? "replay1" : "replay2")}`= @replay WHERE `player1` = {battle.P1.Id} AND `player2` = {battle.P2.Id} AND `round` = {battle.Round}", conn);
                    cmd.Parameters.Add("replay", MySqlDbType.String).Value = input;

                    await cmd.ExecuteNonQueryAsync();
                }
                catch (Exception e)
                {
                    Bot.DebugLogger.LogMessage(DSharpPlus.LogLevel.Critical, "", e.ToString(), DateTime.Now);
                    await context.Channel.SendMessageAsync("", false, new DiscordEmbedBuilder()
                    {
                        Color = DiscordColor.Red,
                        Title = "Battle Replay Submission",
                        Description = $"An error occured while processing your request. Please contact a staff member."
                    });
                    return;
                }
                finally
                {
                    await conn.CloseAsync();
                }
                await context.Channel.SendMessageAsync("", false, new DiscordEmbedBuilder()
                {
                    Color = Bot._config.Color,
                    Title = $"Round {battle.Round} Battle Replay Submission",
                    Description = "Replay submitted! :white_check_mark:"
                });
            }
            #endregion
        }
    }
}