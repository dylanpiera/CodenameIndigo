using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectIndigoPlus.Modules.ModelModule
{
    public class TourneyModel
    {
        public DateTimeOffset RegStart { get; set; }
        public DateTimeOffset RegEnd { get; set; }
        public DateTimeOffset Closure { get; set; }
        public int Tid { get; set; }
        public int MinPlayers { get; set; }
        public int MaxPlayers { get; set; }
        public int PlayerCount { get; set; }
        public string Name { get; set; }
    }
}
