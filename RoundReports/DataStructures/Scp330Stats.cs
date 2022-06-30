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
    public class Scp330Stats : IReportStat
    {
        public string Title => "SCP-330 Stats";
        public int Order => 75;
        public DateTime FirstUse { get; set; } = DateTime.MinValue;
        public int TotalCandiesTaken { get; set; }
        public int SeveredHands { get; set; }
        public Dictionary<CandyKindID, int> CandiesTaken { get; set; } = new(0);
        public Dictionary<Player, int> CandiesByPlayer { get; set; } = new(0);
    }
}
