using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using MySql.Data.MySqlClient;
using ProjectIndigoPlus.Entities;
using ProjectIndigoPlus.Modules.HelperModule;
using ProjectIndigoPlus.Modules.ModelModule;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectIndigoPlus.Modules.Commands
{
    internal class Deregister
    {
        private readonly Dependencies dep;
        public Deregister(Dependencies d)
        {
            dep = d;
        }

        [Command("deregister"),
         Aliases(new[] { "signoff", "unregister", "leave" }),
         Description("Deregister from the current tournament.")]
        public async Task DeregisterAsync(CommandContext context)
        {
            DiscordChannel channel;
            if (!context.Channel.IsPrivate)
            {
                context.RespondAndDelete(new DiscordEmbedBuilder()
                {
                    Color = Bot._config.Color,
                    Title = "Tournament Registration",
                    Description = $"Hey {context.User.Mention}! I'll be sending you a DM with the details :)"
                }, TimeSpan.FromSeconds(20));
                channel = (await context.Member.CreateDmChannelAsync());
            }
            else
            {
                channel = context.Channel;
            }

            /// Checks what tourneys are available for deregistration
            List<TourneyModel> tournaments = new List<TourneyModel>();
            #region !! Retrieve tournaments from Database !!
            MySqlConnection conn = DatabaseHelper.GetClosedConnection();

            try
            {
                await conn.OpenAsync();

                MySqlCommand cmd = new MySqlCommand($"SELECT `tid`,`tournament`,`regstart`,`regend`,`closure`,`maxplayers`,`minplayers` FROM `tournaments` WHERE `regstart` <= {DateTimeOffset.Now.ToUnixTimeSeconds()} AND `regend` >= {DateTimeOffset.Now.ToUnixTimeSeconds()} ORDER BY `tid` DESC", conn);

                Dictionary<int, TourneyModel> tourneys = new Dictionary<int, TourneyModel>();
                using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        tourneys.Add(reader.GetInt32("tid"), new TourneyModel()
                        {
                            Tid = reader.GetInt32("tid"),
                            Name = reader.GetString("tournament"),
                            RegStart = DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64("regstart")),
                            RegEnd = DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64("regend")),
                            Closure = DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64("closure")),
                            MaxPlayers = reader.GetInt32("maxplayers"),
                            MinPlayers = reader.GetInt32("minplayers")
                        });
                    }
                }

                cmd = new MySqlCommand($"SELECT `tid` FROM `teams` WHERE `uid` = {context.User.Id}", conn);

                using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        try
                        {
                            tournaments.Add(tourneys[reader.GetInt32("tid")]);
                        }
                        catch
                        {

                        }
                    }
                }
            }
            catch (Exception e)
            {
                Bot.DebugLogger.LogMessage(DSharpPlus.LogLevel.Critical, "Deregistration Tournament Retrieval" + "", e.ToString(), DateTime.Now);
            }
            finally
            {
                await conn.CloseAsync();
            }
            #endregion

            TourneyModel tournament = null;
            if (tournaments.Count == 0)
            {
                await channel.SendMessageAsync("", false, new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Red,
                    Title = "No option to unregister",
                    Description = "It appears there is no tournament currently in signups that you have signed up to.\n\nIf you'd like to leave an on-going tournament please contact a Bulbaleague Staff member."
                });
                return;
            }
            else if (tournaments.Count > 1)
            {
                string tourneylist = "";
                Dictionary<int, TourneyModel> tournamentIndex = new Dictionary<int, TourneyModel>();
                int i = 0;
                foreach (TourneyModel tourney in tournaments)
                {
                    tourneylist += $"{++i}. Tournament {tourney.Tid} - {tourney.Name}\n";
                    tournamentIndex.Add(i, tourney);
                }

                DiscordMessage msg = await channel.SendMessageAsync("", false, new DiscordEmbedBuilder()
                {
                    Color = Bot._config.Color,
                    Title = "Deregistration",
                    Description = "Hey there, sad to see you sign off from you current tournament.\n\n" +
                    "Please select the tournament you want to leave from:\n\n" + tourneylist
                });

                for (i = 1; i <= tournaments.Count && i < 8; i++)
                {
                    await msg.CreateReactionAsync(DiscordEmoji.FromName(context.Client, EmojiHelper.EmojiStringFromNumber(i)));
                }

                ReactionContext response = await dep.Interactivity.WaitForMessageReactionAsync(x => EmojiHelper.TryParseString(x.GetDiscordName().Replace(':', ' ').Trim(), out i), msg, context.User);
                if (response == null)
                {
                    await channel.SendMessageAsync("", false, new DiscordEmbedBuilder()
                    {
                        Color = DiscordColor.Red,
                        Title = "Tourney selection",
                        Description = $"It appears I lost you :cry:\n To restart type `{Bot._config.Prefix + context.Command.Name}`"
                    });
                    return;
                }
                else
                {
                    tournament = tournamentIndex[i];
                }
            }
            else
            {
                tournament = tournaments[0];
            }

            DiscordMessage cfrMessage = await channel.SendMessageAsync("", false, new DiscordEmbedBuilder()
            {
                Color = Bot._config.Color,
                Title = "Tournament Deregistration",
                Description = $"To confirm, you wish to deregister from the {tournament.Name}?"
            });
            await cfrMessage.CreateReactionAsync(DiscordEmoji.FromName(context.Client, ":x:"));
            await cfrMessage.CreateReactionAsync(DiscordEmoji.FromName(context.Client, ":white_check_mark:"));

            ReactionContext react = await dep.Interactivity.WaitForMessageReactionAsync(x => x.GetDiscordName() == ":x:" || x.GetDiscordName() == ":white_check_mark:", cfrMessage, context.User);
            if (react == null)
            {
                await channel.SendMessageAsync("", false, new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Red,
                    Title = "Tournament Deregistration",
                    Description = $"It appears I lost you :cry:\n To restart type `{Bot._config.Prefix + context.Command.Name}`"
                });
                return;
            }
            else if (react.Emoji.GetDiscordName() == ":x:")
            {
                await channel.SendMessageAsync("", false, new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.DarkRed,
                    Title = "Tournament Deregistration",
                    Description = "Okay! Cancelling deregistration."
                });
            }
            else if (react.Emoji.GetDiscordName() == ":white_check_mark:")
            {
                #region !! Remove participation from Database !!

                await conn.OpenAsync();

                string cmdString = "DELETE FROM `teams` WHERE `uid` = @uid AND `tid` = @tid";
                MySqlCommand cmd = new MySqlCommand(cmdString, conn);
                cmd.Parameters.Add("uid", MySqlDbType.UInt64).Value = context.User.Id;
                cmd.Parameters.Add("tid", MySqlDbType.Int32).Value = tournament.Tid;

                try
                {
                    await cmd.ExecuteNonQueryAsync();
                }
                catch (Exception e)
                {
                    context.Client.DebugLogger.LogMessage(DSharpPlus.LogLevel.Critical, "CheckUserInDatabase @ Signup Command", e.ToString(), DateTime.Now);
                    await channel.SendMessageAsync("An error occured. Please contact an administrator.");
                    return;
                }
                finally
                {
                    await conn.CloseAsync();
                }

                #endregion

                await channel.SendMessageAsync("", false, new DiscordEmbedBuilder()
                {
                    Color = Bot._config.Color,
                    Title = "Tournament Deregistration",
                    Description = "Okay! I have removed you from the list of registered players. Maybe until next time!"
                });
            }
        }
    }
}
