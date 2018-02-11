using Discord;
using Discord.Commands;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CodenameIndigo.Modules.Preconditions
{
    class InSignupPrecon : PreconditionAttribute
    {
        public async override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            MySqlConnection conn = ConnectionTest.GetClosedConnection();
            long UNIXTime = 0;
            try
            {
                await conn.OpenAsync();

                MySqlCommand cmd = new MySqlCommand("SELECT `regend` FROM `tournaments` WHERE `tid` = 1", conn);

                using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        UNIXTime = reader.GetInt64(0);
                    }
                }
            }
            catch (Exception e)
            {
                await Program.Log(e.ToString(), "SignupDate SQL", LogSeverity.Error);
            }
            finally
            {
                await conn.CloseAsync();
            }
            if (UNIXTime > DateTimeOffset.Now.ToUnixTimeSeconds())
                return PreconditionResult.FromSuccess();
            await context.Channel.SendMessageAsync("Signups for the current tournament aren't open.");
            return PreconditionResult.FromError("");
        }
    }

    class NotInSignupPrecon : PreconditionAttribute
    {
        public async override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            MySqlConnection conn = ConnectionTest.GetClosedConnection();
            long UNIXTime = 0;
            try
            {
                await conn.OpenAsync();

                MySqlCommand cmd = new MySqlCommand("SELECT `end_date` FROM `setup` WHERE `tourney_id` = 1", conn);

                using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        UNIXTime = reader.GetInt64(0);
                    }
                }
            }
            catch (Exception e)
            {
                await Program.Log(e.ToString(), "SignupDate SQL", LogSeverity.Error);
            }
            finally
            {
                await conn.CloseAsync();
            }
            if (UNIXTime > DateTimeOffset.Now.ToUnixTimeSeconds())
            {
                await context.Channel.SendMessageAsync("Signups for the current tournament are still opened.");
                return PreconditionResult.FromError("");
            }
            return PreconditionResult.FromSuccess();
        }
    }
}
