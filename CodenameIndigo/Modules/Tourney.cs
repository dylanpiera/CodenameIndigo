using System;
using System.Collections.Generic;
using System.Text;

namespace CodenameIndigo.Modules
{
    public struct Tourney
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public long Regstart { get; set; }
        public long Regend { get; set; }
        public int MinPlayers { get; set; }
        public int MaxPlayers { get; set; }
    }
}
