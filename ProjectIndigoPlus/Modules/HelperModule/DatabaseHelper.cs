using MySql.Data.MySqlClient;
using ProjectIndigoPlus.Modules.ModelModule;
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

        public static async Task<TourneyModel> GetLatestTourneyAsync(this MySqlConnection conn)
        {
            TourneyModel tourney = null;
            try
            {
                await conn.OpenAsync();
                string commandstring = "SELECT `tid`,`tournament`,`regstart`,`regend`,`closure`,`maxplayers`,`minplayers` FROM `tournaments` ORDER BY `tid` DESC LIMIT 0,1";
                MySqlCommand cmd = new MySqlCommand(commandstring, conn);
                using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        tourney = new TourneyModel()
                        {
                            Tid = reader.GetInt32("tid"),
                            Name = reader.GetString("tournament"),
                            RegStart = DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64("regstart")),
                            RegEnd = DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64("regend")),
                            Closure = DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64("closure")),
                            MaxPlayers = reader.GetInt32("maxplayers"),
                            MinPlayers = reader.GetInt32("minplayers")
                        };
                    }
                }

            }
            catch (Exception e)
            {
                Bot.DebugLogger.LogMessage(DSharpPlus.LogLevel.Critical, "GetLatestTourneyAsync", e.ToString(), DateTime.Now);
                return null;
            }
            finally
            {
                await conn.CloseAsync();
            }
            return tourney;
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

        public static async Task<(bool, TourneyModel)> GetTourneyByIDAsync(this MySqlConnection conn, int id)
        {
            TourneyModel tourney = null;
            bool success = false;
            try
            {
                await conn.OpenAsync();

                MySqlCommand cmd = new MySqlCommand("SELECT `tid`,`tournament`,`regstart`,`regend`,`closure`,`maxplayers`,`minplayers`,COUNT(`tid`) as Amount FROM `tournaments` WHERE `tid` = 20" + id, conn);

                using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        if (reader.GetInt32("Amount") > 0)
                        {
                            tourney = new TourneyModel()
                            {
                                Tid = reader.GetInt32("tid"),
                                Name = reader.GetString("tournament"),
                                RegStart = DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64("regstart")),
                                RegEnd = DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64("regend")),
                                Closure = DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64("closure")),
                                MaxPlayers = reader.GetInt32("maxplayers"),
                                MinPlayers = reader.GetInt32("minplayers")
                            };
                            success = true;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Bot.DebugLogger.LogMessage(DSharpPlus.LogLevel.Critical, "GetTourneyByID", e.ToString(), DateTime.Now);
                return (false, null);
            }
            finally
            {
                await conn.CloseAsync();
            }

            return (success, tourney);
        }

        /*public static async Task<Tourney> GetLatestTourneyAsync()
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