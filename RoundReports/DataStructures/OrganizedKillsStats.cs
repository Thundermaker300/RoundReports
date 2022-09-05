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
        [Translation(nameof(Translation.OrganizedKillsTitle))]
        public string Title => "Kills";
        public int Order => 50;

        [Translation(nameof(Translation.KillsByPlayer))]
        public Dictionary<Player, int> KillsByPlayer { get; set; } = new(0);

        [Translation(nameof(Translation.KillsbyType))]
        public Dictionary<DamageType, int> KillsByType { get; set; } = new(0);

        [Translation(nameof(Translation.PlayerKills))]
        public List<string> PlayerKills { get; set; } = new(0);
    }
}
