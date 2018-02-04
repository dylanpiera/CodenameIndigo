using Discord;
using Discord.Commands;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CodenameIndigo.Modules.Preconditions
{
    class SignupPrecon : PreconditionAttribute
    {
        public async override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            return await GameAcceptingSignups() && await UserNotRegistered(context, context.User.Id) ? PreconditionResult.FromSuccess() : PreconditionResult.FromError($"Game is not Accepting Signups or {context.User.Username} is already signed up.");
        }

        private async Task<bool> UserNotRegistered(ICommandContext context, ulong uuid)
        {
            MySqlConnection conn = ConnectionTest.GetClosedConnection();
            bool isRegistered = false;
            try
            {
                await conn.OpenAsync();

                string cmdString = $"SELECT COUNT(UUID) FROM users WHERE UUID = {uuid}";
                MySqlCommand cmd = new MySqlCommand(cmdString, conn);

                using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        isRegistered = reader.GetInt16(0) > 0 ? false : true;
                    }
                }
            }
            catch (Exception e)
            {
                await Program.Log(new LogMessage(LogSeverity.Error, "UserRegistered - SignupPrecon", e.ToString()));
            }
            finally
            {
                await conn.CloseAsync();
            }

            if(!isRegistered)
            {
                try
                {
                    await context.Message.DeleteAsync();
                }
                catch { }

                await (await context.User.GetOrCreateDMChannelAsync()).SendMessageAsync("You are already registered for the upcoming tournament.");
            }
            return isRegistered;
        }

        private async Task<bool> GameAcceptingSignups()
        {
            await Program.Log("Returning success. - Not yet implemented", "SignupPrecon", Discord.LogSeverity.Debug);
            return true;
        }
    }
}
