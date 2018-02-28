using System;
using System.Collections.Generic;
using System.Text;

namespace CodenameIndigo.Modules.Models
{
    public class Bracket
    {
        public int BID { get; private set; }
        public readonly int TID;
        public int Round = 1;
        public Player Player1 { get; private set; }
        public Player Player2 { get; private set; }
        public int Winner { get; private set; }

        public Bracket(int tID, Player player1, Player player2, int round = 1)
        {
            TID = tID;
            Player1 = player1;
            Player2 = player2;
            Round = round;
            BID = 0;
            Winner = 0;
        }

        public Bracket(int tID, Player player1, int round = 1)
        {
            TID = tID;
            Player1 = player1;
            Round = round;
            BID = 0;
            Winner = 1;
        }
    }
}
