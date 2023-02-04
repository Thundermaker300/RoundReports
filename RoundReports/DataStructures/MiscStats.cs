namespace RoundReports
{
#pragma warning disable SA1600
    using System.Collections.Generic;
    using Exiled.API.Enums;
    using Exiled.API.Features.Pools;

    public class MiscStats : IReportStat
    {
        [Translation(nameof(Translation.MiscTitle))]
        public string Title => "Miscellaneous Statistics";

        public int Order => 8;

        [Header(nameof(Translation.RespawnTitle))]
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

        [Header(nameof(Translation.MapTitle))]
        [Translation(nameof(Translation.TotalRooms))]
        [BindStat(StatType.TotalRooms)]
        public int TotalRooms { get; set; }

        [Translation(nameof(Translation.RoomsByZone))]
        [BindStat(StatType.RoomsByZone)]
        public Dictionary<ZoneType, PercentInt> RoomsByZone { get; set; }

        [Translation(nameof(Translation.TotalCameras))]
        [BindStat(StatType.TotalCameras)]
        public int TotalCameras { get; set; }

        [Translation(nameof(Translation.TotalDoors))]
        [BindStat(StatType.TotalDoors)]
        public int TotalDoors { get; set; }

        [Header(nameof(Translation.TeslaTitle))]
        [Translation(nameof(Translation.TotalTeslaGates))]
        [BindStat(StatType.TotalTeslaGates)]
        public int TotalTeslaGates { get; set; }

        [Translation(nameof(Translation.TeslaShocks))]
        [BindStat(StatType.TeslaShocks)]
        public int TeslaShocks { get; set; }

        [Translation(nameof(Translation.TeslaDamage))]
        [BindStat(StatType.TeslaDamage)]
        public int TeslaDamage { get; set; }

        public void Setup()
        {
            SpawnWaves = ListPool<string>.Pool.Get();
            Respawns = ListPool<string>.Pool.Get();
            RoomsByZone = DictionaryPool<ZoneType, PercentInt>.Pool.Get();
        }

        public void Cleanup()
        {
            ListPool<string>.Pool.Return(SpawnWaves);
            ListPool<string>.Pool.Return(Respawns);
            DictionaryPool<ZoneType, PercentInt>.Pool.Return(RoomsByZone);
        }

        public void FillOutFinal()
        {
            OrganizedDamageStats stats = MainPlugin.Reporter?.GetStat<OrganizedDamageStats>();
            if (stats is not null && stats.DamageByType.TryGetValue(DamageType.Tesla, out PercentInt value))
            {
                TeslaDamage = value.Value;
            }
        }
    }
#pragma warning restore SA1600
}
