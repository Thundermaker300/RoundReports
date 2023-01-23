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
        public Dictionary<ItemType, int> Drops { get; set; }

        [Translation(nameof(Translation.PlayerDrops))]
        [BindStat(StatType.PlayerDrops)]
        public Dictionary<Player, int> PlayerDrops { get; set; }

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
        public Dictionary<FirearmType, int> ShotsByFirearm { get; set; }

        public void Setup()
        {
            Drops = DictionaryPool<ItemType, int>.Pool.Get();
            PlayerDrops = DictionaryPool<Player, int>.Pool.Get();
            ShotsByFirearm = DictionaryPool<FirearmType, int>.Pool.Get();
        }

        public void Cleanup()
        {
            DictionaryPool<ItemType, int>.Pool.Return(Drops);
            DictionaryPool<Player, int>.Pool.Return(PlayerDrops);
            DictionaryPool<FirearmType, int>.Pool.Return(ShotsByFirearm);
        }
    }
#pragma warning restore SA1600
}
