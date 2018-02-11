using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CodenameIndigo.Modules
{
    public static class RandomizationHelper
    {
        public static async Task<List<Player>> GenerateRandomBracketsAsync(int tid = 1, bool rand = true)
        {
            MySqlConnection conn = ConnectionTest.GetClosedConnection();
            List<Player> participants = new List<Player>();

            try
            {
                await conn.OpenAsync();

                MySqlCommand cmd = new MySqlCommand("SELECT * FROM `participants` WHERE `tid` = " + tid, conn);
                if (rand)
                    cmd.CommandText += " ORDER BY RAND()";

                using (MySqlDataReader reader = (MySqlDataReader) await cmd.ExecuteReaderAsync())
                {
                    while(await reader.ReadAsync())
                    {
                        participants.Add(new Player { Id = (ulong)reader.GetInt64(2), DiscordName = reader.GetString(3), ShowdownName = reader.GetString(4), Team = reader.GetString(5)});
                    }
                }
            }
            catch (Exception e)
            {
                await Program.Log(e.ToString(), "GenerateRandomBrackets", Discord.LogSeverity.Error);
            }
            finally
            {
                await conn.CloseAsync();
            }
            return participants;
        }

        //Possibly add multiple types of generation later, like round robin.
    }
}
