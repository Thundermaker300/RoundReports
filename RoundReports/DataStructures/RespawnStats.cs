using Exiled.API.Features.Pools;
using System.Collections.Generic;

namespace RoundReports
{
    public class RespawnStats : IReportStat
    {
        [Translation(nameof(Translation.RespawnTitle))]
        public string Title => "Respawn Statistics";
        public int Order => 6;

        [Rule(Rule.CommaSeparatedList)]
        [Translation(nameof(Translation.SpawnWaves))]
        [BindStat(StatType.SpawnWaves)]
        public List<string> SpawnWaves { get; set; }

        [Translation(nameof(Translation.TotalRespawnedPlayers))]
        [BindStat(StatType.TotalRespawned)]
        public int TotalRespawned { get; set; } = 0;

        [Translation(nameof(Translation.Respawns))]
        [BindStat(StatType.Respawns)]
        public List<string> Respawns { get; set; }

        public void Setup()
        {
            SpawnWaves = ListPool<string>.Pool.Get();
            Respawns = ListPool<string>.Pool.Get();
        }

        public void Cleanup()
        {
            ListPool<string>.Pool.Return(SpawnWaves);
            ListPool<string>.Pool.Return(Respawns);
        }
    }
}
