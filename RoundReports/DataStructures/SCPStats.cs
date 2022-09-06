using Exiled.API.Features;
using InventorySystem.Items.Usables.Scp330;
using Scp914;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoundReports
{
    public class SCPStats : IReportStat
    {
        [Translation(nameof(Translation.ScpItemTitle))]
        public string Title => "SCP Statistics";
        public int Order => 6;

        [Header("SCP Item Usages")]
        [Translation(nameof(Translation.Scp018Thrown))]
        public int Scp018Thrown { get; set; }

        [Translation(nameof(Translation.Scp207Drank))]
        public int Scp207Drank { get; set; }

        [Translation(nameof(Translation.Scp268Uses))]
        public int Scp268Uses { get; set; }

        [Translation(nameof(Translation.Scp1853Uses))]
        public int Scp1853Uses { get; set; }

        [Header("SCP-330 Statistics")]
        [Translation(nameof(Translation.FirstUse))]
        public DateTime FirstUse { get; set; } = DateTime.MinValue;

        [Translation(nameof(Translation.FirstUser))]
        public Player FirstUser { get; set; }

        [Translation(nameof(Translation.TotalCandiesTaken))]
        public int TotalCandiesTaken { get; set; }

        [Translation(nameof(Translation.SeveredHands))]
        public int SeveredHands { get; set; }

        [Translation(nameof(Translation.CandiesTaken))]
        public Dictionary<CandyKindID, int> CandiesTaken { get; set; } = new(0);

        [Translation(nameof(Translation.CandiesByPlayer))]
        public Dictionary<Player, int> CandiesByPlayer { get; set; } = new(0);

        [Header("SCP-914 Statistics")]
        [Translation(nameof(Translation.FirstActivation))]
        public DateTime FirstActivation { get; set; } = DateTime.MinValue;

        [Translation(nameof(Translation.FirstActivator))]
        public Player FirstActivator { get; set; }

        [Translation(nameof(Translation.TotalActivations))]
        public int TotalActivations { get; set; }

        [Translation(nameof(Translation.TotalItemUpgrades))]
        public int TotalItemUpgrades { get; set; }

        [Translation(nameof(Translation.KeycardUpgrades))]
        public int KeycardUpgrades { get; set; }

        [Translation(nameof(Translation.FirearmUpgrades))]
        public int FirearmUpgrades { get; set; }

        [Translation(nameof(Translation.Activations))]
        public Dictionary<Scp914KnobSetting, int> Activations { get; set; } = new()
        {
            [Scp914KnobSetting.Rough] = 0,
            [Scp914KnobSetting.Coarse] = 0,
            [Scp914KnobSetting.OneToOne] = 0,
            [Scp914KnobSetting.Fine] = 0,
            [Scp914KnobSetting.VeryFine] = 0,
        };

        [Translation(nameof(Translation.Upgrades))]
        public Dictionary<ItemType, int> Upgrades { get; set; } = new();
    }
}
