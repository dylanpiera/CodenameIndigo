using MySql.Data.MySqlClient;
using System;
using System.Threading.Tasks;

namespace IndigoBot.Modules.Helpers
{
    public static class DatabaseHelper
    {
        public static MySqlConnection GetClosedConnection()
        {
            string connStr = $"Server={Bot._config.DbServer};Uid={Bot._config.DbUser};Database={Bot._config.DbName};port=3306;Password={Bot._config.DbPass};SslMode=none;CharSet=utf8mb4";
            MySqlConnection conn = new MySqlConnection(connStr) { };
            return conn;
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