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
        [Translation(nameof(Translation.Scp330Title))]
        public string Title => "SCP-330 Stats";
        public int Order => 75;

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
    }
}
