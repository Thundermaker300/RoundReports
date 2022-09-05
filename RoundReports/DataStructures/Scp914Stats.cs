using Exiled.API.Enums;
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
    public class Scp914Stats : IReportStat
    {
        [Translation(nameof(Translation.Scp914Title))]
        public string Title => "SCP-914 Stats";
        public int Order => 76;

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
