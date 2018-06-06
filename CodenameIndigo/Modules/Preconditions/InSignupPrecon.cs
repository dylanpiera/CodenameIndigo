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
            MySqlConnection conn = DatabaseHelper.GetClosedConnection();
            long UNIXTime = 0;
            try
            {
                await conn.OpenAsync();

                MySqlCommand cmd = new MySqlCommand("SELECT `regend` FROM `tournaments` WHERE `tid` = " + (await DatabaseHelper.GetLatestTourneyAsync()).ID, conn);

                using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        UNIXTime = reader.GetInt64("regend");
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
        private int _tournamentID;

        public NotInSignupPrecon()
        {

        }

        public NotInSignupPrecon(int id)
        {
            _tournamentID = id;
        }

        public async override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if(_tournamentID == 0)
                _tournamentID = (await DatabaseHelper.GetLatestTourneyAsync()).ID;
            MySqlConnection conn = DatabaseHelper.GetClosedConnection();
            long UNIXTime = 0;
            try
            {
                await conn.OpenAsync();

                MySqlCommand cmd = new MySqlCommand("SELECT `regend` FROM `tournaments` WHERE `tid` = " + _tournamentID, conn);

                using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        UNIXTime = reader.GetInt64("regend");
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
