using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using MySql.Data.MySqlClient;
using ProjectIndigoPlus.Entities;
using ProjectIndigoPlus.Modules.HelperModule;
using System;
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
            switch (await CheckUserInDatabase(context))
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
                    Title = "First time registration",
                    Description = "It appears I lost you :( To restart the progress type `?register`"
                });
                return;
            }
            else
            {
                string showdownusername = msg.Message.Content.Split(' ')[0];
                if (showdownusername.ToCharArray().Length >= 19)
                {
                    await channel.SendMessageAsync("", false, new DiscordEmbedBuilder()
                    {
                        Color = DiscordColor.Red,
                        Title = "Showdown Username",
                        Description = "Oops! It appears your input isn't a showdown username! Please tell me your showdown username."
                    });
                    context.Client.DebugLogger.LogMessage(DSharpPlus.LogLevel.Debug, "Showdown Username Receiver", $"{context.User.Username} submitted a faulty username: {showdownusername}", DateTime.Now);
                    goto RequestShowdownUsername;
                }
                DiscordMessage confirmMessage = await channel.SendMessageAsync("", false, new DiscordEmbedBuilder()
                {
                    Color = Bot._config.Color,
                    Title = "Username Confirmation",
                    Description = $"So your Showdown username is: {showdownusername}?"
                });
                await confirmMessage.CreateReactionAsync(DiscordEmoji.FromName(context.Client, ":x:"));
                await confirmMessage.CreateReactionAsync(DiscordEmoji.FromName(context.Client, ":white_check_mark:"));

                ReactionContext reaction = await dep.Interactivity.WaitForMessageReactionAsync(x => x.GetDiscordName() == ":x:" || x.GetDiscordName() == ":white_check_mark:", confirmMessage, context.User);
                if (reaction == null)
                {
                    await channel.SendMessageAsync("", false, new DiscordEmbedBuilder()
                    {
                        Color = DiscordColor.Red,
                        Title = "First time registration",
                        Description = "It appears I lost you :( To restart the progress type `?register`"
                    });
                    return;
                }
                else if (reaction.Emoji.GetDiscordName() == ":x:")
                {
                    await channel.SendMessageAsync("", false, new DiscordEmbedBuilder()
                    {
                        Color = Bot._config.Color,
                        Title = "Showdown Username",
                        Description = "I see. Then could you please tell me what your Showdown username is?"
                    });
                    goto RequestShowdownUsername;
                }
                else if (reaction.Emoji.GetDiscordName() == ":white_check_mark:")
                {
                    await channel.SendMessageAsync("", false, new DiscordEmbedBuilder()
                    {
                        Color = Bot._config.Color,
                        Title = "Showdown Username",
                        Description = "Awesome! I'll process your account registration. Now onto your tournament registration!"
                    });

                    #region !! Add User to Database !!
                    MySqlConnection conn = DatabaseHelper.GetClosedConnection();

                    await conn.OpenAsync();

                    string cmdString = "INSERT INTO `members`(`uid`, `discordusername`, `showdownusername`, `avatar`) VALUES (@uid,@discordname,@showdownusername,@avatar)";
                    MySqlCommand cmd = new MySqlCommand(cmdString, conn);
                    cmd.Parameters.Add("uid", MySqlDbType.UInt32).Value = context.User.Id;
                    cmd.Parameters.Add("discordusername", MySqlDbType.VarChar).Value = context.User.Username;
                    cmd.Parameters.Add("showdownusername", MySqlDbType.VarChar).Value = showdownusername;
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
            
            Success:



            return;
        }

        /// <summary>
        /// Check if invoker of command is already registered in our database as a member.
        /// </summary>
        /// <param name="context">Command Context</param>
        /// <returns>true/false wheter use exists in database</returns>
        public async Task<SuccessValue> CheckUserInDatabase(CommandContext context)
        {
            // [ DEBUG ONLY ]
            return SuccessValue.failure;
            // [ REMOVE WHEN DONE DEBUGGING]

            SuccessValue response = SuccessValue.error;
            MySqlConnection conn = DatabaseHelper.GetClosedConnection();

            try
            {
                await conn.OpenAsync();

                string cmdString = "SELECT COUNT(uid) FROM `members` WHERE `uid` = @uid";
                MySqlCommand cmd = new MySqlCommand(cmdString, conn);
                cmd.Parameters.Add("uid", MySqlDbType.UInt32).Value = context.User.Id;

                using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        response = reader.GetInt16(0) > 0 ? SuccessValue.success : SuccessValue.failure;
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

            return response;
        }
    }
}