namespace RoundReports
{
#pragma warning disable SA1600
    using System.Collections.Generic;
    using Exiled.API.Enums;
    using Exiled.API.Features;
    using Exiled.API.Features.Pools;

    public class OrganizedKillsStats : IReportStat
    {
        [Translation(nameof(Translation.OrganizedKillsTitle))]
        public string Title => "Kills";

        public int Order => 50;

        [Translation(nameof(Translation.KillsByPlayer))]
        [BindStat(StatType.KillsByPlayer)]
        public Dictionary<Player, PercentInt> KillsByPlayer { get; set; }

        [Translation(nameof(Translation.KillsbyType))]
        [BindStat(StatType.KillsByType)]
        public Dictionary<DamageType, PercentInt> KillsByType { get; set; }

        [Translation(nameof(Translation.KillsByZone))]
        [BindStat(StatType.KillsByZone)]
        public Dictionary<ZoneType, PercentInt> KillsByZone { get; set; }

        [Translation(nameof(Translation.PlayerKills))]
        [BindStat(StatType.PlayerKills)]
        public List<string> PlayerKills { get; set; }

        public void Setup()
        {
            PlayerKills = ListPool<string>.Pool.Get();
            KillsByPlayer = DictionaryPool<Player, PercentInt>.Pool.Get();
            KillsByType = DictionaryPool<DamageType, PercentInt>.Pool.Get();
            KillsByZone = DictionaryPool<ZoneType, PercentInt>.Pool.Get();
        }

        public void Cleanup()
        {
            ListPool<string>.Pool.Return(PlayerKills);
            DictionaryPool<Player, PercentInt>.Pool.Return(KillsByPlayer);
            DictionaryPool<DamageType, PercentInt>.Pool.Return(KillsByType);
            DictionaryPool<ZoneType, PercentInt>.Pool.Return(KillsByZone);
        }
    }
#pragma warning restore SA1600
}
