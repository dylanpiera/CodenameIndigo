using System;
using System.Collections.Generic;
using System.Text;

namespace CodenameIndigo.Modules
{
    public struct Player
    {
        public ulong Id { get; set; }
        public string DiscordName { get; set; }
        public string ShowdownName { get; set; }
        public string Team { get; set; }
    }
}
