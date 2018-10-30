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
    internal class Signup
    {
        private Dependencies dep;
        public Signup(Dependencies d)
        {
            dep = d;
        }

        [Command("register"),
         Aliases(new string[] { "join", "signup" }),
         Description("Register for a tournament! Be sure to have your showdown team ready!")]
        public async Task RegisterAsync(CommandContext context)
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

            await channel.SendMessageAsync($"Hey {context.User.Username}! Loading data... please give me a moment :)");
            await channel.TriggerTypingAsync();
            MySqlConnection conn = DatabaseHelper.GetClosedConnection();
            bool updateUsername = false;
            int tid = 0;

            (SuccessValue, string) result = await CheckUserInDatabase(context);
            string showdownusername = result.Item2;

            switch (result.Item1)
            {
                case SuccessValue.error:
                    goto Error;
                case SuccessValue.failure:
                    goto Failure;
                case SuccessValue.success:
                    goto Success;
            }

            /// <summary>
            /// If a database error occurs break function.
            /// </summary>
            Error:
            await channel.SendMessageAsync("It appears there was a server error :(\n" +
                "Contact an administrator or try again later!");
            return;

            Failure:
            await channel.SendMessageAsync("", false, new DiscordEmbedBuilder()
            {
                Color = Bot._config.Color,
                Title = "First time registration",
                Description = "Howdy, I noticed it's your first time signing up for the BulbaLeague! First of all welcome!\n\n" +
                "Before we continue, I'd like to know your Showdown name, could you tell it to me?",
                Footer = new DiscordEmbedBuilder.EmbedFooter() { Text = "By signing up to the BulbaLeague you accept our Terms and Services: https://sdus.eu/bltos" }
            });

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
            else if(msg.Message.Content.ToLower() == "exit")
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
                    if (updateUsername)
                    {
                        await channel.SendMessageAsync("", false, new DiscordEmbedBuilder()
                        {
                            Color = Bot._config.Color,
                            Title = "Showdown Username Update",
                            Description = "Okay! I'll update your username information, now why don't you continue signing up for the tournament!\n\n" +
                            "Please tell me your team for the tournament. Either paste it here in the chat (feel free to style it in a code block!) or send it to me as a .txt file!"
                        });

                        #region !! Update user in Database !!

                        await conn.OpenAsync();

                        string cmdString = "UPDATE `members` SET `discordusername`=@discordusername,`showdownusername`=@showdownusername,`avatar`=@avatar WHERE `uid` = @uid";
                        MySqlCommand cmd = new MySqlCommand(cmdString, conn);
                        cmd.Parameters.Add("uid", MySqlDbType.UInt64).Value = context.User.Id;
                        cmd.Parameters.Add("discordusername", MySqlDbType.VarChar).Value = context.User.Username;
                        cmd.Parameters.Add("showdownusername", MySqlDbType.VarChar).Value = newUsername;
                        cmd.Parameters.Add("avatar", MySqlDbType.VarChar).Value = context.User.GetAvatarUrl(DSharpPlus.ImageFormat.Gif);

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
                    }
                    else
                    {
                        await channel.SendMessageAsync("", false, new DiscordEmbedBuilder()
                        {
                            Color = Bot._config.Color,
                            Title = "Showdown Username",
                            Description = "Awesome! I'll process your account registration. Now onto your tournament registration!"
                        });

                        #region !! Add User to Database !!

                        await conn.OpenAsync();

                        string cmdString = "INSERT INTO `members`(`uid`, `discordusername`, `showdownusername`, `avatar`) VALUES (@uid,@discordname,@showdownusername,@avatar)";
                        MySqlCommand cmd = new MySqlCommand(cmdString, conn);
                        cmd.Parameters.Add("uid", MySqlDbType.UInt64).Value = context.User.Id;
                        cmd.Parameters.Add("discordusername", MySqlDbType.VarChar).Value = context.User.Username;
                        cmd.Parameters.Add("showdownusername", MySqlDbType.VarChar).Value = newUsername;
                        cmd.Parameters.Add("avatar", MySqlDbType.VarChar).Value = context.User.GetAvatarUrl(DSharpPlus.ImageFormat.Gif);

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
                    }
                }
            }
            if (updateUsername)
            {
                updateUsername = false;
                goto RetryTeam;
            }

            Success:

            /// Checks what tourneys are open for signups
            Dictionary<int, TourneyModel> tournaments = new Dictionary<int, TourneyModel>();

            #region !! Retrieve tournaments from Database !!
            try
            {
                await conn.OpenAsync();

                MySqlCommand cmd = new MySqlCommand($"SELECT `tid`,`tournament`,`regstart`,`regend`,`closure`,`maxplayers`,`minplayers` FROM `tournaments` WHERE `regstart` <= {DateTimeOffset.Now.ToUnixTimeSeconds()} AND `regend` >= {DateTimeOffset.Now.ToUnixTimeSeconds()} ORDER BY `tid` DESC", conn);

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

                cmd = new MySqlCommand($"SELECT `tid` FROM `teams` WHERE `uid` = {context.User.Id}", conn);

                using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        tournaments.Remove(reader.GetInt32("tid"));
                    }
                }
            }
            catch (Exception e)
            {
                Bot.DebugLogger.LogMessage(DSharpPlus.LogLevel.Critical, "" + "", e.ToString(), DateTime.Now);
            }
            finally
            {
                await conn.CloseAsync();
            }
            #endregion

            if (tournaments.Count == 0)
            {
                await channel.SendMessageAsync("", false, new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Red,
                    Title = "No tourney's open for signup",
                    Description = "Aww... it appears there are no tournaments that you can sign up for :cry:\n\n" +
                    "Maybe try again later?"
                });
                return;
            }
            #region !! Display Tournament Picker !!
            DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
            {
                Color = Bot._config.Color,
                Title = "Pick a tournament",
                Description = "The following tourneys are open for signups:\n\n",
                Footer = new DiscordEmbedBuilder.EmbedFooter() { Text = "Please select a tournament by sending its number!" }
            };

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
            else if(ctx.Message.Content.ToLower() == "exit")
            {
                return;
            }

            int.TryParse(ctx.Message.Content, out tid);

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
                Description = $"Cool! Let's signup for the {tournaments[tid].Name}!\n\n" +
                $"First, let me confirm your showdown name, is it still {showdownusername}?"
            });

            await confirmMessage.CreateReactionAsync(DiscordEmoji.FromName(context.Client, ":x:"));
            await confirmMessage.CreateReactionAsync(DiscordEmoji.FromName(context.Client, ":white_check_mark:"));

            ReactionContext reaction = await dep.Interactivity.WaitForMessageReactionAsync(x => x.GetDiscordName() == ":x:" || x.GetDiscordName() == ":white_check_mark:", confirmMessage, context.User);
            if (reaction == null)
            {
                await channel.SendMessageAsync("", false, new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Red,
                    Title = "Tourney Signup",
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

                updateUsername = true;

                goto RequestShowdownUsername;
            }

            await channel.SendMessageAsync("", false, new DiscordEmbedBuilder()
            {
                Color = Bot._config.Color,
                Title = "Tourney Signup",
                Description = "Awesome! Then please tell me your team for the tournament. Either paste it here in the chat (feel free to style it in a code block!) or send it to me as a .txt file!"
            });
            RetryTeam:
            string team = "";
            MessageContext teamMessage = await dep.Interactivity.WaitForMessageAsync(x => x.Author.Id == context.User.Id && x.Channel.Id == channel.Id);
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
                        Description = "It appears the file you send me isn't a safe file. Please send me your team as a .txt or send the team as a discord message!"
                    });
                    goto RetryTeam;
                }
            }
            else if(teamMessage.Message.Content.ToLower() == "exit")
            {
                return;
            }
            if (string.IsNullOrEmpty(team))
            {
                team = teamMessage.Message.Content.Replace('`',' ').Trim();
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
                #region !! Save registration to Database !!

                await conn.OpenAsync();

                string cmdString = $"INSERT INTO `teams`(`uid`, `tid`, `team`, `regdate`) VALUES (@uid,@tid,@team,{DateTimeOffset.Now.ToUnixTimeSeconds()})";
                MySqlCommand cmd = new MySqlCommand(cmdString, conn);
                cmd.Parameters.Add("uid", MySqlDbType.UInt64).Value = context.User.Id;
                cmd.Parameters.Add("tid", MySqlDbType.UInt32).Value = tid;
                cmd.Parameters.Add("team", MySqlDbType.Text).Value = team;

                try
                {
                    await cmd.ExecuteNonQueryAsync();
                }
                catch (Exception e)
                {
                    context.Client.DebugLogger.LogMessage(DSharpPlus.LogLevel.Critical, "SaveTeamToDatabase @ Signup Command", e.ToString(), DateTime.Now);
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
                    Description = $"Awesome, {context.User.Username}!\nThank you for signing up!\n\n" +
                    $"I will let you know when the tournament begins and give you information from there!\n" +
                    $"If you'd like to change your entry. Please visit our website https://bulbaleague.soaringnetwork.com \n\n" +
                    $"Good luck, and keep fighting!"
                });
            }
        }

        /// <summary>
        /// Check if invoker of command is already registered in our database as a member.
        /// </summary>
        /// <param name="context">Command Context</param>
        /// <returns>true/false wheter use exists in database</returns>
        public async Task<(SuccessValue, string)> CheckUserInDatabase(CommandContext context)
        {
            SuccessValue response = SuccessValue.error;
            MySqlConnection conn = DatabaseHelper.GetClosedConnection();
            string showdownusername = "";

            try
            {
                await conn.OpenAsync();

                string cmdString = "SELECT COUNT(uid),`showdownusername` FROM `members` WHERE `uid` = @uid";
                MySqlCommand cmd = new MySqlCommand(cmdString, conn);
                cmd.Parameters.Add("uid", MySqlDbType.UInt64).Value = context.User.Id;

                using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        response = reader.GetInt16(0) > 0 ? SuccessValue.success : SuccessValue.failure;
                        showdownusername = reader.GetString("showdownusername");
                    }
                }
            }
            catch (Exception e)
            {
                context.Client.DebugLogger.LogMessage(DSharpPlus.LogLevel.Critical, "CheckUserInDatabase @ Signup Command", e.ToString(), DateTime.Now);
                response = SuccessValue.error;
            }
            finally
            {
                await conn.CloseAsync();
            }

            return (response, showdownusername);
        }
    }
}