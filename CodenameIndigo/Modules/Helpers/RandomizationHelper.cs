using CodenameIndigo.Modules.Models;
using Discord;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodenameIndigo.Modules
{
    public static class RandomizationHelper
    {
        public static async Task<List<Player>> GenerateBrackets(int tourneyID)
        {
            MySqlConnection conn = DatabaseHelper.GetClosedConnection();
            List<Player> participants = new List<Player>();

            try
            {
                await conn.OpenAsync();

                int maxPlayers = 0;
                MySqlCommand cmd = new MySqlCommand("SELECT `maxplayers` FROM `tournaments` WHERE `tid` = " + tourneyID, conn);

                using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        maxPlayers = reader.GetInt32("maxplayers");
                    }
                }

                cmd = new MySqlCommand($"SELECT * FROM `participants` WHERE `tid` = {tourneyID} ORDER BY `pid` ASC LIMIT 0,{maxPlayers}", conn);

                using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        participants.Add(new Player
                        {
                            Id = (ulong)reader.GetInt64("uuid"),
                            Pid = reader.GetInt32("pid"),
                            DiscordName = reader.GetString("discordusername"),
                            ShowdownName = reader.GetString("showdownusername"),
                            Team = reader.GetString("team")
                        });
                    }
                }
            }
            catch (Exception e)
            {
                await Program.Log(e.ToString(), "GenerateBrackets", Discord.LogSeverity.Error);
            }
            finally
            {
                await conn.CloseAsync();
            }
            return participants;
        }

        public static async void RandomizeBrackets(this List<Player> participants, int tid)
        {
            List<Player> shuffledPlayers = (List<Player>)participants.Shuffle();
            List<Bracket> brackets = new List<Bracket>();

            if (shuffledPlayers.Count % 2 != 0)
            {
                brackets.Add(new Bracket(tid, shuffledPlayers[shuffledPlayers.Count - 1]));
                shuffledPlayers.RemoveAt(shuffledPlayers.Count);
            }

            for (int i = 0; i < shuffledPlayers.Count; i += 2)
            {
                brackets.Add(new Bracket(tid, shuffledPlayers[i], shuffledPlayers[i + 1]));
            }


            MySqlConnection conn = DatabaseHelper.GetClosedConnection();
            try
            {
                await conn.OpenAsync();
                
                foreach (Bracket item in brackets)
                {
                    MySqlCommand cmd = new MySqlCommand($"INSERT INTO `battles`(`tid`, `round`, `player1`, `player2`, `winner`) VALUES ({item.TID},{item.Round},{item.Player1.Pid},{item.Player2.Pid},{item.Winner})", conn);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            catch (Exception e)
            {
                await Program.Log(e.ToString(), "DatabaseConn", LogSeverity.Error);
            }
            finally
            {
                await conn.CloseAsync();
            }
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, Int32? seed = null)
        {
            List<T> buffer = source.ToList();

            Random random = seed.HasValue ? new Random(seed.Value) : new Random();

            Int32 count = buffer.Count;

            for (Int32 i = 0; i < count; i++)
            {
                Int32 j = random.Next(i, count);
                yield return buffer[j];
                buffer[j] = buffer[i];
            }
        }


        //Possibly add multiple types of generation later, like round robin.
    }
}
