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
        public string Title => "Warhead Statistics";
        public int Order => 8;
        public bool ButtonUnlocked { get; set; } = false;
        public Player ButtonUnlocker { get; set; }
        public Player FirstActivator { get; set; }
        public bool Detonated { get; set; } = false;
        public DateTime DetonationTime { get; set; } = DateTime.MinValue;
    }
}
