using Exiled.API.Enums;
using Exiled.API.Features;
using InventorySystem.Items.Usables.Scp330;
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
        public int TotalActivations { get; set; }
        public int TotalItemUpgrades { get; set; }

    }
}
