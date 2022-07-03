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
        public string Title => "SCP-914 Stats";
        public int Order => 76;
        public DateTime FirstActivation { get; set; } = DateTime.MinValue;
        public Player FirstActivator { get; set; }
        public int TotalActivations { get; set; }
        public int TotalItemUpgrades { get; set; }
        public int KeycardUpgrades { get; set; }
        public int FirearmUpgrades { get; set; }
        public Dictionary<Scp914KnobSetting, int> Activations { get; set; } = new()
        {
            [Scp914KnobSetting.Rough] = 0,
            [Scp914KnobSetting.Coarse] = 0,
            [Scp914KnobSetting.OneToOne] = 0,
            [Scp914KnobSetting.Fine] = 0,
            [Scp914KnobSetting.VeryFine] = 0,
        };
        public Dictionary<ItemType, int> Upgrades { get; set; } = new();

    }
}
