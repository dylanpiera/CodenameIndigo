using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectIndigoPlus.Modules.ModelModule
{
    public class BattleModel
    {
        public int Round { get; set; }
        public Player P1 { get; set; }
        public Player P2 { get; set; }
        public string Replay1 { get; set; }
        public string Replay2 { get; set; }
    }

    public class Player
    {
        readonly public ulong Id;
        readonly public string Name;

        public Player(ulong id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
