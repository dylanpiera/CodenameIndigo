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
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ProjectIndigoPlus.Modules.Commands
{
    internal class EditRegistration
    {
        private readonly Dependencies dep;
        public EditRegistration(Dependencies d)
        {
            dep = d;
        }

        [Command("editsignup"),
            Aliases(new[] { "editregistration", "editteam" }),
            Description("Allows you to change your signup for a tournament you already registered to. The tourney must still be accepting signups.")]
        public async Task Command(CommandContext context, string input = "")
        {
            DiscordChannel channel;

            if (!context.Channel.IsPrivate)
            {
                await context.Message.CreateReactionAsync(DiscordEmoji.FromName(context.Client, ":white_check_mark:"));
                context.RespondAndDelete(new DiscordEmbedBuilder()
                {
                    Color = Bot._config.Color,
                    Title = "Edit Tournament Registration",
                    Description = $"Hey {context.User.Mention}! I'll be sending you a DM with the details :)"
                }, TimeSpan.FromSeconds(20));
                channel = (await context.Member.CreateDmChannelAsync());
            }
            else
            {
                channel = context.Channel;
            }

            Dictionary<int, TourneyModel> tournaments = new Dictionary<int, TourneyModel>();
            MySqlConnection conn = DatabaseHelper.GetClosedConnection();
            try
            {
                await conn.OpenAsync();

                MySqlCommand cmd = new MySqlCommand($"SELECT `tournaments`.`tid`,`tournaments`.`tournament`,`tournaments`.`regstart`,`tournaments`.`regend`,`tournaments`.`closure`,`tournaments`.`maxplayers`,`tournaments`.`minplayers` FROM `tournaments` LEFT JOIN `teams` ON `tournaments`.`tid` = `teams`.`tid` WHERE `teams`.`uid` = {context.User.Id} AND `regstart` <= {DateTimeOffset.Now.ToUnixTimeSeconds()} AND `regend` >= {DateTimeOffset.Now.ToUnixTimeSeconds()} ORDER BY `tournaments`.`tid` DESC", conn);

                using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                {

                    while (await reader.ReadAsync())
                    {
                        tournaments.Add(reader.GetInt32("tid"), new TourneyModel()
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
            }
            catch (Exception e)
            {
                Bot.DebugLogger.LogMessage(DSharpPlus.LogLevel.Critical, "Edit Signup Tournament Retrieval" + "", e.ToString(), DateTime.Now);
            }
            finally
            {
                await conn.CloseAsync();
            }

            if (tournaments.Count == 0)
            {
                await channel.SendMessageAsync("", false, new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Red,
                    Title = "No available tournaments",
                    Description = "Aww... it appears there are no tournament signups that you can edit. :cry:\n\n" +
                    "Please keep in mind you can only edit a signup before the tournament starts."
                });
                return;
            }
            #region !! Display Tournament Picker !!
            DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
            {
                Color = Bot._config.Color,
                Title = "Pick a tournament",
                Footer = new DiscordEmbedBuilder.EmbedFooter() { Text = "Please select a tournament by sending its number!" }
            };

            builder.Description = "The following tourneys are available:\n\n";
            foreach (TourneyModel tourney in tournaments.Values)
            {
                builder.Description += $"{tourney.Tid}. {tourney.Name} - [{tourney.PlayerCount}/{tourney.MaxPlayers}]\n";
            }

            await channel.SendMessageAsync("", false, builder.Build());
            #endregion

            RetryPicker:
            MessageContext ctx = await dep.Interactivity.WaitForMessageAsync(x =>
            {
                return x.Channel.Id == channel.Id &&
                x.Author.Id == context.User.Id &&
                Regex.IsMatch(x.Content, @"^\d+$");
            }
            );
            if (ctx == null)
            {
                await channel.SendMessageAsync("", false, new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Red,
                    Title = "Tourney Selection",
                    Description = $"It appears I lost you :cry:\n To restart type `{Bot._config.Prefix + context.Command.Name}`"
                });
                return;
            }
            else if (ctx.Message.Content.ToLower() == "exit")
            {
                return;
            }

            int.TryParse(ctx.Message.Content, out int tid);

            if (!tournaments.ContainsKey(tid))
            {
                await channel.SendMessageAsync("", false, new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Red,
                    Title = "Tourney Selection",
                    Description = $"It appears that tournament with ID {ctx.Message.Content} doesn't excist, could you give me one of the numbers listed from one of the tournaments above?"
                });
                goto RetryPicker;
            }

            DiscordMessage confirmMessage = await channel.SendMessageAsync("", false, new DiscordEmbedBuilder()
            {
                Color = Bot._config.Color,
                Title = "Tourney Signup",
                Description = $"Cool! Let's edit your signup for the {tournaments[tid].Name}!\n\n"
            });
            await EditRegistrationAsync(context, dep, channel, tournaments[tid]);
        }

        internal static async Task EditRegistrationAsync(CommandContext context, Dependencies dep, DiscordChannel channel, TourneyModel tourney, string showdownusername = "")
        {
            if (string.IsNullOrEmpty(showdownusername))
            {
                showdownusername = (await Register.CheckUserInDatabase(context)).Item2;
            }

            DiscordMessage confirmMessage = await channel.SendMessageAsync("", false, new DiscordEmbedBuilder()
            {
                Color = Bot._config.Color,
                Title = "Edit Tourney Registration",
                Description = $"First, let me confirm your showdown name, is it still {showdownusername}?"
            });

            await confirmMessage.CreateReactionAsync(DiscordEmoji.FromName(context.Client, ":x:"));
            await confirmMessage.CreateReactionAsync(DiscordEmoji.FromName(context.Client, ":white_check_mark:"));

            ReactionContext reaction = await dep.Interactivity.WaitForMessageReactionAsync(x => x.GetDiscordName() == ":x:" || x.GetDiscordName() == ":white_check_mark:", confirmMessage, context.User);
            if (reaction == null)
            {
                await channel.SendMessageAsync("", false, new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Red,
                    Title = "Edit Tourney Registration",
                    Description = $"It appears I lost you :cry:\n To restart type `{Bot._config.Prefix + context.Command.Name}`"
                });
                return;
            }
            else if (reaction.Emoji.GetDiscordName() == ":x:")
            {
                await channel.SendMessageAsync("", false, new DiscordEmbedBuilder()
                {
                    Color = Bot._config.Color,
                    Title = "Tourney Signup",
                    Description = "I see. Then could you please tell me what your new Showdown username is?"
                });

                #region !! Update Showdown Username
                RequestShowdownUsername:
                MessageContext msg = await dep.Interactivity.WaitForMessageAsync(x => x.ChannelId == channel.Id && x.Author.Id == context.User.Id);

                if (msg == null)
                {
                    await channel.SendMessageAsync("", false, new DiscordEmbedBuilder()
                    {
                        Color = DiscordColor.Red,
                        Title = "Showdown username registration",
                        Description = $"It appears I lost you :cry:\n To restart type `{Bot._config.Prefix + context.Command.Name}`"
                    });
                    return;
                }
                else if (msg.Message.Content.ToLower() == "exit")
                {
                    return;
                }
                else
                {
                    string newUsername = msg.Message.Content.Split(' ')[0];
                    if (newUsername.ToCharArray().Length >= 19)
                    {
                        await channel.SendMessageAsync("", false, new DiscordEmbedBuilder()
                        {
                            Color = DiscordColor.Red,
                            Title = "Showdown Username",
                            Description = "Oops! It appears your input isn't a showdown username! Please tell me your showdown username."
                        });
                        context.Client.DebugLogger.LogMessage(DSharpPlus.LogLevel.Debug, "Showdown Username Receiver", $"{context.User.Username} submitted a faulty username: {newUsername}", DateTime.Now);
                        goto RequestShowdownUsername;
                    }
                    DiscordMessage cfrMessage = await channel.SendMessageAsync("", false, new DiscordEmbedBuilder()
                    {
                        Color = Bot._config.Color,
                        Title = "Username Confirmation",
                        Description = $"So your Showdown username is: {newUsername}?"
                    });
                    await cfrMessage.CreateReactionAsync(DiscordEmoji.FromName(context.Client, ":x:"));
                    await cfrMessage.CreateReactionAsync(DiscordEmoji.FromName(context.Client, ":white_check_mark:"));

                    ReactionContext react = await dep.Interactivity.WaitForMessageReactionAsync(x => x.GetDiscordName() == ":x:" || x.GetDiscordName() == ":white_check_mark:", cfrMessage, context.User);
                    if (react == null)
                    {
                        await channel.SendMessageAsync("", false, new DiscordEmbedBuilder()
                        {
                            Color = DiscordColor.Red,
                            Title = "First time registration",
                            Description = $"It appears I lost you :cry:\n To restart type `{Bot._config.Prefix + context.Command.Name}`"
                        });
                        return;
                    }
                    else if (react.Emoji.GetDiscordName() == ":x:")
                    {
                        await channel.SendMessageAsync("", false, new DiscordEmbedBuilder()
                        {
                            Color = Bot._config.Color,
                            Title = "Showdown Username",
                            Description = "I see. Then could you please tell me what your Showdown username is?"
                        });
                        goto RequestShowdownUsername;
                    }
                    else if (react.Emoji.GetDiscordName() == ":white_check_mark:")
                    {

                        await channel.SendMessageAsync("", false, new DiscordEmbedBuilder()
                        {
                            Color = Bot._config.Color,
                            Title = "Showdown Username Update",
                            Description = "Okay! I'll update your username information, now why don't you continue editing your signup for the tournament!"
                        });

                        MySqlConnection conn = DatabaseHelper.GetClosedConnection();
                        #region !! Update user in Database !!

                        await conn.OpenAsync();

                        string cmdString = "UPDATE `members` SET `discordusername`=@discordusername,`showdownusername`=@showdownusername,`avatar`=@avatar WHERE `uid` = @uid";
                        MySqlCommand cmd = new MySqlCommand(cmdString, conn);
                        cmd.Parameters.Add("uid", MySqlDbType.UInt64).Value = context.User.Id;
                        cmd.Parameters.Add("discordusername", MySqlDbType.VarChar).Value = context.User.Username + "#" + context.User.Discriminator;
                        cmd.Parameters.Add("showdownusername", MySqlDbType.VarChar).Value = newUsername;
                        cmd.Parameters.Add("avatar", MySqlDbType.VarChar).Value = context.User.GetAvatarUrl(DSharpPlus.ImageFormat.Gif);

                        try
                        {
                            await cmd.ExecuteNonQueryAsync();
                        }
                        catch (Exception e)
                        {
                            context.Client.DebugLogger.LogMessage(DSharpPlus.LogLevel.Critical, "UpdateUserInDatabase @ EditRegistration Command", e.ToString(), DateTime.Now);
                            await channel.SendMessageAsync("An error occured. Please contact an administrator.");
                            return;
                        }
                        finally
                        {
                            await conn.CloseAsync();
                        }

                        #endregion

                    }
                }
                #endregion
            }

            await channel.SendMessageAsync("", false, new DiscordEmbedBuilder()
            {
                Color = Bot._config.Color,
                Title = "Edit Tourney Registration",
                Description = "Please tell me your new team for the tournament. Either paste it here in the chat (feel free to style it in a code block!) or send it to me as a .txt file!",
                Footer = new DiscordEmbedBuilder.EmbedFooter() {Text = "Type `exit` to cancel editing your team." }
            });
            RetryTeam:
            string team = "";
            MessageContext teamMessage = await dep.Interactivity.WaitForMessageAsync(x => x.Author.Id == context.User.Id && x.Channel.Id == channel.Id);
            if (teamMessage == null)
            {
                await channel.SendMessageAsync("", false, new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Red,
                    Title = "Team Registration",
                    Description = $"It appears I lost you :cry:\n To restart type `{Bot._config.Prefix + context.Command.Name}`"
                });
                return;
            }
            if (teamMessage.Message.Attachments.Count > 0)
            {
                DiscordAttachment attachment = teamMessage.Message.Attachments[0];
                if (attachment.FileName.EndsWith(".txt") && attachment.FileSize < 5000)
                {
                    await channel.SendMessageAsync("", false, new DiscordEmbedBuilder()
                    {
                        Color = Bot._config.Color,
                        Title = "File Received!",
                        Description = "I got your file, let me read it for a moment!\n:eyeglasses:\n:book:"
                    });

                    WebRequest webRequest = WebRequest.Create(attachment.Url);
                    string strContent = "";

                    using (WebResponse response = webRequest.GetResponse())
                    using (Stream content = response.GetResponseStream())
                    using (StreamReader reader = new StreamReader(content))
                    {
                        strContent = reader.ReadToEnd();
                    }

                    if (string.IsNullOrEmpty(strContent))
                    {
                        await channel.SendMessageAsync("", false, new DiscordEmbedBuilder()
                        {
                            Color = DiscordColor.Red,
                            Title = "File empty?",
                            Description = "It appears the file you send me is empty or corrupted. Please try again or send the message to me as text!"
                        });
                        goto RetryTeam;
                    }
                    team = strContent;
                }
                else
                {
                    await channel.SendMessageAsync("", false, new DiscordEmbedBuilder()
                    {
                        Color = DiscordColor.Red,
                        Title = "Wrong File",
                        Description = "It appears the file you send me isn't a safe file. Please send me your team as a .txt or send the team as a discord message!",
                        Footer = new DiscordEmbedBuilder.EmbedFooter() { Text = @"The default showdown team storage location for the desktop application is `C:\Users\{your user}\documents\My Games\Pokemon Showdown\Teams`" }
                    });
                    goto RetryTeam;
                }
            }
            else if (teamMessage.Message.Content.ToLower() == "exit")
            {
                return;
            }
            if (string.IsNullOrEmpty(team))
            {
                team = teamMessage.Message.Content.Replace('`', ' ').Trim();
            }

            DiscordMessage teamConfirmMessage = await channel.SendMessageAsync("", false, new DiscordEmbedBuilder()
            {
                Color = Bot._config.Color,
                Title = "Team confirmation",
                Description = $"So your team is: ```{team}```?"
            });

            await teamConfirmMessage.CreateReactionAsync(DiscordEmoji.FromName(context.Client, ":x:"));
            await teamConfirmMessage.CreateReactionAsync(DiscordEmoji.FromName(context.Client, ":white_check_mark:"));

            ReactionContext teamReaction = await dep.Interactivity.WaitForMessageReactionAsync(x => x.GetDiscordName() == ":x:" || x.GetDiscordName() == ":white_check_mark:", teamConfirmMessage, context.User);
            if (teamReaction == null)
            {
                await channel.SendMessageAsync("", false, new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Red,
                    Title = "Team confirmation",
                    Description = $"It appears I lost you :cry:\n To restart type `{Bot._config.Prefix + context.Command.Name}`"
                });
                return;
            }
            else if (teamReaction.Emoji.GetDiscordName() == ":x:")
            {
                await channel.SendMessageAsync("", false, new DiscordEmbedBuilder()
                {
                    Color = Bot._config.Color,
                    Title = "Team confirmation",
                    Description = "I see. Could you please send me your real team then?"
                });

                goto RetryTeam;
            }
            else
            {
                #region !! Update registration in Database !!
                MySqlConnection conn = DatabaseHelper.GetClosedConnection();
                try
                {
                    await conn.OpenAsync();

                    string cmdString = $"UPDATE `teams` SET `team`=@team,`checked`=null WHERE `tid` = @tid AND `uid` = @uid";
                    MySqlCommand cmd = new MySqlCommand(cmdString, conn);
                    cmd.Parameters.Add("uid", MySqlDbType.UInt64).Value = context.User.Id;
                    cmd.Parameters.Add("tid", MySqlDbType.UInt32).Value = tourney.Tid;
                    cmd.Parameters.Add("team", MySqlDbType.Text).Value = team;


                    await cmd.ExecuteNonQueryAsync();
                }
                catch (Exception e)
                {
                    context.Client.DebugLogger.LogMessage(DSharpPlus.LogLevel.Critical, "UpdateTeamInDatabase @ EditRegistration Command", e.ToString(), DateTime.Now);
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
                    Title = "Signup Complete!",
                    Description = $"Awesome, {context.User.Username}!\nI hope all is correct now!\n\n" +
                    $"I will let you know when the tournament begins and give you information from there!\n" +
                    $"Good luck, and keep fighting!"
                });

            }
        }
    }
}
