using NorthwoodLib.Pools;
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
            SpawnWaves = ListPool<string>.Shared.Rent();
            Respawns = ListPool<string>.Shared.Rent();
        }

        public void Cleanup()
        {
            ListPool<string>.Shared.Return(SpawnWaves);
            ListPool<string>.Shared.Return(Respawns);
        }
    }
}
