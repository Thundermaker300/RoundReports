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
        public Dictionary<Player, int> KillsByPlayer { get; set; }

        [Translation(nameof(Translation.KillsbyType))]
        [BindStat(StatType.KillsByType)]
        public Dictionary<DamageType, int> KillsByType { get; set; }

        [Translation(nameof(Translation.KillsByZone))]
        [BindStat(StatType.KillsByZone)]
        public Dictionary<ZoneType, int> KillsByZone { get; set; }

        [Translation(nameof(Translation.PlayerKills))]
        [BindStat(StatType.PlayerKills)]
        public List<string> PlayerKills { get; set; }

        public void Setup()
        {
            PlayerKills = ListPool<string>.Pool.Get();
            KillsByPlayer = DictionaryPool<Player, int>.Pool.Get();
            KillsByType = DictionaryPool<DamageType, int>.Pool.Get();
            KillsByZone = DictionaryPool<ZoneType, int>.Pool.Get();
        }

        public void Cleanup()
        {
            ListPool<string>.Pool.Return(PlayerKills);
            DictionaryPool<Player, int>.Pool.Return(KillsByPlayer);
            DictionaryPool<DamageType, int>.Pool.Return(KillsByType);
            DictionaryPool<ZoneType, int>.Pool.Return(KillsByZone);
        }
    }
#pragma warning restore SA1600
}
