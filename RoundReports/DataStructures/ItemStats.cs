using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoundReports
{
    public class ItemStats : IReportStat
    {
        [Translation(nameof(Translation.ItemStatsTitle))]
        public string Title => "Item Statistics";
        public int Order => 5;
        [Header(nameof(Translation.ItemTransfersTitle))]
        [Translation(nameof(Translation.TotalDrops))]
        public int TotalDrops { get; set; } = 0;

        [Translation(nameof(Translation.Drops))]
        public Dictionary<ItemType, int> Drops { get; set; } = new(0);

        [Translation(nameof(Translation.PlayerDrops))]
        public Dictionary<Player, int> PlayerDrops { get; set; } = new(0);

        [Header(nameof(Translation.ItemUsesTitle))]
        [Translation(nameof(Translation.KeycardScans))]
        public int KeycardScans { get; set; }

        [Translation(nameof(Translation.PainkillersConsumed))]
        public int PainkillersConsumed { get; set; }

        [Translation(nameof(Translation.MedkitsConsumed))]
        public int MedkitsConsumed { get; set; }

        [Translation(nameof(Translation.AdrenalinesConsumed))]
        public int AdrenalinesConsumed { get; set; }

        [Translation(nameof(Translation.Scp500sConsumed))]
        public int SCP500sConsumed { get; set; }
    }
}
