using Discord;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CodenameIndigo.Modules.Models
{
    public class Bracket
    {
        public int BID { get; private set; }
        public readonly int TID;
        public int Round = 1;
        public int Player1 { get; private set; }
        public int Player2 { get; private set; }
        public int Winner { get; private set; }

        public Bracket(int tID, int player1, int player2, int round = 1, int bid = 0, int winner = 0)
        {
            TID = tID;
            Player1 = player1;
            Player2 = player2;
            Round = round;
            BID = bid;
            Winner = winner;
        }

        public Bracket(int tID, int player1, int round = 1)
        {
            TID = tID;
            Player1 = player1;
            Round = round;
            BID = 0;
            Winner = 1;
        }

        public async Task<Player> FetchPlayerAsync(int p)
        {
            int pid;
            if (p == 1)
            {
                pid = this.Player1;
            }
            else if (p == 2)
            {
                if(this.Player2 == 0)
                {
                    return new Player() { Pid = 0, DiscordName = "Bye" };
                }
                pid = this.Player2;
            }
            else
            {
                throw new ArgumentException("p isn't 1 or 2.");
            }
            Player player = new Player();
            MySqlConnection conn = DatabaseHelper.GetClosedConnection();
            try
            {
                await conn.OpenAsync();
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM `participants` WHERE `pid` = " + pid, conn);
                using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        player = new Player()
                        {
                            Pid = pid,
                            DiscordName = reader.GetString("discordusername"),
                            Id = reader.GetUInt64("uid"),
                            ShowdownName = reader.GetString("showdownusername"),
                            Team = reader.GetString("team")
                        };
                    }
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
            return player;
        }
    }
}
