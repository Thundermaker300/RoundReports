using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoundReports
{
    public class WarheadStats : IReportStat
    {
        [Translation(nameof(Translation.WarheadStatsTitle))]
        public string Title => "Warhead Statistics";
        public int Order => 8;

        [Translation(nameof(Translation.ButtonUnlocked))]
        public bool ButtonUnlocked { get; set; } = false;

        [Translation(nameof(Translation.ButtonUnlocker))]
        public Player ButtonUnlocker { get; set; }

        [Translation(nameof(Translation.FirstActivator))]
        public Player FirstActivator { get; set; }

        [Translation(nameof(Translation.Detonated))]
        public bool Detonated { get; set; } = false;

        [Translation(nameof(Translation.DetonationTime))]
        public DateTime DetonationTime { get; set; } = DateTime.MinValue;
    }
}
