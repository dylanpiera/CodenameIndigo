using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectIndigoPlus.Modules.HelperModule
{
    public static class DatabaseHelper
    {
        /// <summary>
        /// Retrieves a closed copy of the global MySqlConnection
        /// </summary>
        public static MySqlConnection GetClosedConnection()
        {
            string connStr = $"Server={Bot._config.DbServer};Uid={Bot._config.DbUser};Database={Bot._config.DbName};port=3306;Password={Bot._config.DbPass};SslMode=none;CharSet=utf8mb4";
            MySqlConnection conn = new MySqlConnection(connStr) { };
            return conn;
        }

        /// <summary>
        /// Gets a single row worth of data
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="commandstring"></param>
        /// <returns></returns>
        public static async Task<Dictionary<string, object>> GetRowDataFromDBAsync(this MySqlConnection conn, string commandstring)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            try
            {
                await conn.OpenAsync();
                MySqlCommand cmd = new MySqlCommand(commandstring, conn);
                using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                {
                    await reader.ReadAsync();

                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        dictionary.Add(reader.GetName(i), reader.GetValue(i));
                    }
                }

            }
            catch (Exception e)
            {
                Bot.DebugLogger.LogMessage(DSharpPlus.LogLevel.Critical, "GetDataFromDBAsync with cmdString: " + commandstring, e.ToString(), DateTime.Now);
                return null;
            }
            finally
            {
                await conn.CloseAsync();
            }
            return dictionary;
        }

        /*public static async Task<Tourney> GetTourneyByIDAsync(int id)
        {
            Tourney tourney = new Tourney();

            MySqlConnection conn = DatabaseHelper.GetClosedConnection();
            try
            {
                await conn.OpenAsync();

                MySqlCommand cmd = new MySqlCommand("SELECT * FROM `tournaments` WHERE `tid` = " + id, conn);

                using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        tourney.ID = reader.GetInt32("tid");
                        tourney.Name = reader.GetString("tournament");
                        tourney.Regstart = reader.GetInt32("regstart");
                        tourney.Regend = reader.GetInt32("regend");
                        tourney.MinPlayers = reader.GetInt32("minplayers");
                        tourney.MaxPlayers = reader.GetInt32("maxplayers");
                    }
                }
            }
            catch (Exception e)
            {
                await Program.Log(e.ToString(), "GetTourneyByID => SQL", LogSeverity.Error);
            }
            finally
            {
                await conn.CloseAsync();
            }

            return tourney;
        }

        public static async Task<Tourney> GetLatestTourneyAsync()
        {
            Tourney tourney = new Tourney();

            MySqlConnection conn = DatabaseHelper.GetClosedConnection();
            try
            {
                await conn.OpenAsync();

                MySqlCommand cmd = new MySqlCommand("SELECT * FROM `tournaments` ORDER BY `regstart` DESC LIMIT 0, 1", conn);

                using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        tourney.ID = reader.GetInt32("tid");
                        tourney.Name = reader.GetString("tournament");
                        tourney.Regstart = reader.GetInt32("regstart");
                        tourney.Regend = reader.GetInt32("regend");
                        tourney.MinPlayers = reader.GetInt32("minplayers");
                        tourney.MaxPlayers = reader.GetInt32("maxplayers");
                    }
                }
            }
            catch (Exception e)
            {
                await Program.Log(e.ToString(), "GetLatestTourney => Database", LogSeverity.Error);
            }
            finally
            {
                await conn.CloseAsync();
            }
            return tourney;
        }*/
    }
}