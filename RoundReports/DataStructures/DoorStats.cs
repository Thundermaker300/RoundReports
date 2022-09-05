using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoundReports
{
    public class DoorStats : IReportStat
    {
        [Translation(nameof(Translation.DoorStatsTitle))]
        public string Title => "Door Statistics";
        public int Order => 8;

        [Translation(nameof(Translation.DoorsOpened))]
        public int DoorsOpened { get; set; }

        [Translation(nameof(Translation.DoorsClosed))]
        public int DoorsClosed { get; set; }

        [Translation(nameof(Translation.DoorsDestroyed))]
        public int DoorsDestroyed { get; set; }

        [Translation(nameof(Translation.PlayerDoorsOpened))]
        public Dictionary<Player, int> PlayerDoorsOpened { get; set; } = new(0);

        [Translation(nameof(Translation.PlayerDoorsClosed))]
        public Dictionary<Player, int> PlayerDoorsClosed { get; set; } = new(0);
    }
}
