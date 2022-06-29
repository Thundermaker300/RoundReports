using Exiled.API.Enums;
using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoundReports
{
    public class OrganizedKillsStats : IReportStat
    {
        public string Title => "Kills";
        public int Order => 50;

        public Dictionary<Player, int> KillsByPlayer { get; set; } = new(0);
        public Dictionary<DamageType, int> KillsByType { get; set; } = new(0);
        public List<string> PlayerKills { get; set; } = new(0);
    }
}
