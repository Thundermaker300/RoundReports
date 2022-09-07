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
        [Translation(nameof(Translation.ScpTitle))]
        public string Title => "SCP Statistics";
        public int Order => 6;

        [Translation(nameof(Translation.FemurBreakerActivated))]
        public bool FemurBreakerActivated { get; set; }

        [HideIfDefault]
        [Translation(nameof(Translation.Scp096Charges))]
        public int Scp096Charges { get; set; }

        [HideIfDefault]
        [Translation(nameof(Translation.Scp096Enrages))]
        public int Scp096Enrages { get; set; }

        [HideIfDefault]
        [Translation(nameof(Translation.Scp106Teleports))]
        public int Scp106Teleports { get; set; }

        [HideIfDefault]
        [Translation(nameof(Translation.Scp173Blinks))]
        public int Scp173Blinks { get; set; }

        [Header(nameof(Translation.ScpItemTitle))]
        [Translation(nameof(Translation.Scp018Thrown))]
        public int Scp018Thrown { get; set; }

        [Translation(nameof(Translation.Scp207Drank))]
        public int Scp207Drank { get; set; }

        [Translation(nameof(Translation.Scp268Uses))]
        public int Scp268Uses { get; set; }

        [Translation(nameof(Translation.Scp1853Uses))]
        public int Scp1853Uses { get; set; }

        [Header(nameof(Translation.Scp330Title))]
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

        [Header(nameof(Translation.Scp914Title))]
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
