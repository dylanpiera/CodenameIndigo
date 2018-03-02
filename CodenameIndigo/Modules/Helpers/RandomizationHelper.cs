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
        public static async Task<List<Player>> GetPlayersInTourney(int tourneyID)
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
                            Id = (ulong)reader.GetInt64("uid"),
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
                await Program.Log(e.ToString(), "GetPlayersInTourney", Discord.LogSeverity.Error);
            }
            finally
            {
                await conn.CloseAsync();
            }
            return participants;
        }

        public static async Task<bool> GenerateRandomBrackets(this List<Player> participants, int tid)
        {
            List<Player> shuffledPlayers = participants.Shuffle().ToList<Player>();
            List<Bracket> brackets = new List<Bracket>();
            bool ok;

            if (shuffledPlayers.Count % 2 != 0)
            {
                await Program.Log($"giving {shuffledPlayers[shuffledPlayers.Count-1].Pid} a bye");
                brackets.Add(new Bracket(tid, shuffledPlayers[shuffledPlayers.Count - 1].Pid));
                shuffledPlayers.RemoveAt(shuffledPlayers.Count-1);
            }

            for (int i = 0; i < shuffledPlayers.Count; i += 2)
            {
                await Program.Log($"Pairing {shuffledPlayers[i].Pid} & {shuffledPlayers[i+1].Pid}");
                brackets.Add(new Bracket(tid, shuffledPlayers[i].Pid, shuffledPlayers[i + 1].Pid));
            }

            MySqlConnection conn = DatabaseHelper.GetClosedConnection();
            try
            {
                await conn.OpenAsync();

                foreach (Bracket item in brackets)
                {
                    MySqlCommand cmd = new MySqlCommand($"INSERT INTO `battles`(`tid`, `round`, `player1`, `player2`, `winner`) VALUES ({item.TID},{item.Round},{item.Player1},{item.Player2},{item.Winner})", conn);
                    await cmd.ExecuteNonQueryAsync();
                }
                ok = true;
            }
            catch (Exception e)
            {
                await Program.Log(e.ToString(), "GenerateRandomBrackets", LogSeverity.Error);
                ok = false;
            }
            finally
            {
                await conn.CloseAsync();
            }
            return ok;
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, Int32? seed = null)
        {
            List<T> buffer = source.ToList();

            Random random;

            random = seed.HasValue ? new Random(seed.Value) : new Random();
            
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
