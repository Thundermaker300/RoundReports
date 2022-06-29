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
        public string Title => "Door Statistics";
        public int Order => 8;
        public int DoorsOpened { get; set; }
        public int DoorsClosed { get; set; }
        public int DoorsDestroyed { get; set; }
        public Dictionary<Player, int> PlayerDoorsOpened { get; set; } = new(0);
        public Dictionary<Player, int> PlayerDoorsClosed { get; set; } = new(0);
    }
}
