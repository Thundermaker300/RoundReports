namespace RoundReports
{
#pragma warning disable SA1600
    using System.Collections.Generic;
    using Exiled.API.Enums;
    using Exiled.API.Features;
    using Exiled.API.Features.Pools;

    public class ItemStats : IReportStat
    {
        [Translation(nameof(Translation.ItemStatsTitle))]
        public string Title => "Item Statistics";

        public int Order => 5;

        [Header(nameof(Translation.ItemTransfersTitle))]
        [Translation(nameof(Translation.TotalDrops))]
        [BindStat(StatType.TotalDrops)]
        public int TotalDrops { get; set; } = 0;

        [Translation(nameof(Translation.Drops))]
        [BindStat(StatType.Drops)]
        public Dictionary<ItemType, PercentInt> Drops { get; set; }

        [Translation(nameof(Translation.PlayerDrops))]
        [BindStat(StatType.PlayerDrops)]
        public Dictionary<Player, PercentInt> PlayerDrops { get; set; }

        [Header(nameof(Translation.ItemUsesTitle))]
        [Translation(nameof(Translation.KeycardScans))]
        [BindStat(StatType.KeycardScans)]
        public int KeycardScans { get; set; }

        [Translation(nameof(Translation.PainkillersConsumed))]
        [BindStat(StatType.PainkillersConsumed)]
        public int PainkillersConsumed { get; set; }

        [Translation(nameof(Translation.MedkitsConsumed))]
        [BindStat(StatType.MedkitsConsumed)]
        public int MedkitsConsumed { get; set; }

        [Translation(nameof(Translation.AdrenalinesConsumed))]
        [BindStat(StatType.AdrenalinesConsumed)]
        public int AdrenalinesConsumed { get; set; }

        [Translation(nameof(Translation.Scp500sConsumed))]
        [BindStat(StatType.SCP500sConsumed)]
        public int SCP500sConsumed { get; set; }

        [Header(nameof(Translation.FirearmTitle))]
        [Translation(nameof(Translation.TotalShotsFired))]
        [BindStat(StatType.TotalShotsFired)]
        public int TotalShotsFired { get; set; }

        [Translation(nameof(Translation.TotalReloads))]
        [BindStat(StatType.TotalReloads)]
        public int TotalReloads { get; set; }

        [Translation(nameof(Translation.ShotsByFirearm))]
        [BindStat(StatType.ShotsByFirearm)]
        public Dictionary<FirearmType, PercentInt> ShotsByFirearm { get; set; }

        public void Setup()
        {
            Drops = DictionaryPool<ItemType, PercentInt>.Pool.Get();
            PlayerDrops = DictionaryPool<Player, PercentInt>.Pool.Get();
            ShotsByFirearm = DictionaryPool<FirearmType, PercentInt>.Pool.Get();
        }

        public void Cleanup()
        {
            foreach (var kvp in Drops)
                PercentIntPool.Pool.Return(kvp.Value);

            foreach (var kvp in PlayerDrops)
                PercentIntPool.Pool.Return(kvp.Value);

            DictionaryPool<ItemType, PercentInt>.Pool.Return(Drops);
            DictionaryPool<Player, PercentInt>.Pool.Return(PlayerDrops);
            DictionaryPool<FirearmType, PercentInt>.Pool.Return(ShotsByFirearm);
        }
    }
#pragma warning restore SA1600
}
