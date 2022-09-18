using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoundReports
{
    public class RespawnStats : IReportStat
    {
        [Translation(nameof(Translation.RespawnTitle))]
        public string Title => "Respawn Statistics";
        public int Order => 6;

        [Rule(Rule.CommaSeparatedList)]
        [Translation(nameof(Translation.SpawnWaves))]
        public List<string> SpawnWaves { get; set; } = new();

        [Translation(nameof(Translation.TotalRespawnedPlayers))]
        public int TotalRespawnedPlayers { get; set; } = 0;

        [Translation(nameof(Translation.Respawns))]
        public List<string> Respawns { get; set; } = new();
    }
}
