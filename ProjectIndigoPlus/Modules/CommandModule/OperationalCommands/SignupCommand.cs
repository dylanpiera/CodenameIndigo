using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using MySql.Data.MySqlClient;
using ProjectIndigoPlus.Modules.HelperModule;
using System;
using System.Threading.Tasks;

namespace ProjectIndigoPlus.Modules.Commands
{
    internal class Signup
    {

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

            await channel.SendMessageAsync($"Hey {context.User.Username}! Please give me a moment!");
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
            await channel.SendMessageAsync("I'm afraid there was an error. Please contact an administrator.");
            return;

            Failure:


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
            SuccessValue response = SuccessValue.error;
            MySqlConnection conn = DatabaseHelper.GetClosedConnection();

            try
            {
                await conn.OpenAsync();

                string cmdString = "SELECT COUNT(uid) FROM `members` WHERE `uid` = @uuid";
                MySqlCommand cmd = new MySqlCommand(cmdString);
                cmd.Parameters.Add("uuid", MySqlDbType.UInt32).Value = context.User.Id;

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