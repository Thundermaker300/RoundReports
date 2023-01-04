using Exiled.API.Features;
using InventorySystem.Items.Usables.Scp330;
using Scp914;
using System;
using System.Collections.Generic;

namespace RoundReports
{
    public class SCPStats : IReportStat
    {
        [Translation(nameof(Translation.ScpTitle))]
        public string Title => "SCP Statistics";
        public int Order => 7;

        [HideIfDefault]
        [Translation(nameof(Translation.Scp096Charges))]
        [BindStat(StatType.Scp096Charges)]
        public int Scp096Charges { get; set; }

        [HideIfDefault]
        [Translation(nameof(Translation.Scp096Enrages))]
        [BindStat(StatType.Scp096Enrages)]
        public int Scp096Enrages { get; set; }

        [HideIfDefault]
        [Translation(nameof(Translation.Scp106Teleports))]
        [BindStat(StatType.Scp106Teleports)]
        public int Scp106Teleports { get; set; }

        [HideIfDefault]
        [Translation(nameof(Translation.Scp173Blinks))]
        [BindStat(StatType.Scp173Blinks)]
        public int Scp173Blinks { get; set; }

        [Header(nameof(Translation.ScpItemTitle))]
        [Translation(nameof(Translation.Scp018Thrown))]
        [BindStat(StatType.Scp018sThrown)]
        public int Scp018Thrown { get; set; }

        [Translation(nameof(Translation.Scp207Drank))]
        [BindStat(StatType.Scp207sDrank)]
        public int Scp207Drank { get; set; }

        [Translation(nameof(Translation.Scp268Uses))]
        [BindStat(StatType.Scp268Uses)]
        public int Scp268Uses { get; set; }

        [Translation(nameof(Translation.Scp1853Uses))]
        [BindStat(StatType.Scp1853Uses)]
        public int Scp1853Uses { get; set; }

        [Header(nameof(Translation.Scp330Title))]
        [Translation(nameof(Translation.FirstUse))]
        [BindStat(StatType.First330Use)]
        public DateTime FirstUse { get; set; } = DateTime.MinValue;

        [Translation(nameof(Translation.FirstUser))]
        [BindStat(StatType.First330User)]
        public Player FirstUser { get; set; }

        [Translation(nameof(Translation.TotalCandiesTaken))]
        [BindStat(StatType.TotalCandiesTaken)]
        public int TotalCandiesTaken { get; set; }

        [HideIfDefault]
        [Translation(nameof(Translation.SeveredHands))]
        [BindStat(StatType.SeveredHands)]
        public int SeveredHands { get; set; }

        [Translation(nameof(Translation.CandiesTaken))]
        [BindStat(StatType.CandiesTaken)]
        public Dictionary<CandyKindID, int> CandiesTaken { get; set; }

        [Translation(nameof(Translation.CandiesByPlayer))]
        [BindStat(StatType.CandiesByPlayer)]
        public Dictionary<Player, int> CandiesByPlayer { get; set; }

        [Header(nameof(Translation.Scp914Title))]
        [Translation(nameof(Translation.FirstActivation))]
        [BindStat(StatType.First914Activation)]
        public DateTime FirstActivation { get; set; } = DateTime.MinValue;

        [Translation(nameof(Translation.FirstActivator))]
        [BindStat(StatType.First914Activator)]
        public Player FirstActivator { get; set; }

        [Translation(nameof(Translation.TotalActivations))]
        [BindStat(StatType.Total914Activations)]
        public int TotalActivations { get; set; }

        [Translation(nameof(Translation.TotalItemUpgrades))]
        [BindStat(StatType.TotalItemUpgrades)]
        public int TotalItemUpgrades { get; set; }

        [Translation(nameof(Translation.KeycardUpgrades))]
        [BindStat(StatType.KeycardUpgrades)]
        public int KeycardUpgrades { get; set; }

        [Translation(nameof(Translation.FirearmUpgrades))]
        [BindStat(StatType.FirearmUpgrades)]
        public int FirearmUpgrades { get; set; }

        [Translation(nameof(Translation.Activations))]
        [BindStat(StatType.AllActivations)]
        public Dictionary<Scp914KnobSetting, int> Activations { get; set; }

        [Translation(nameof(Translation.Upgrades))]
        [BindStat(StatType.AllUpgrades)]
        public Dictionary<ItemType, int> Upgrades { get; set; } = new();

        public void Setup()
        {
            CandiesTaken = new();
            CandiesByPlayer = new();
            Activations = new()
            {
                [Scp914KnobSetting.Rough] = 0,
                [Scp914KnobSetting.Coarse] = 0,
                [Scp914KnobSetting.OneToOne] = 0,
                [Scp914KnobSetting.Fine] = 0,
                [Scp914KnobSetting.VeryFine] = 0,
            };
            Upgrades = new();
        }

        public void Cleanup()
        {
        }
    }
}
