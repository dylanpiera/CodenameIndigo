using Discord;
using Discord.Commands;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CodenameIndigo.Modules.Preconditions
{
    class UserNotRegisteredPrecon : PreconditionAttribute
    {
        public async override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            MySqlConnection conn = ConnectionTest.GetClosedConnection();
            bool isRegistered = false;
            try
            {
                await conn.OpenAsync();

                string cmdString = $"SELECT COUNT(UUID) FROM users WHERE UUID = {context.User.Id}";
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

            if (!isRegistered)
            {
                try
                {
                    await context.Message.DeleteAsync();
                }
                catch { }

                await (await context.User.GetOrCreateDMChannelAsync()).SendMessageAsync("You are already registered for the upcoming tournament.");
            }
            return isRegistered ? PreconditionResult.FromSuccess() : null;
        }
    }
}
