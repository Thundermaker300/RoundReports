using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoundReports
{
    public class StartingStats : IReportStat
    {
        public string Title => "Starting Statistics";
        public int Order => 1;
        public string StartTime { get; set; }
        public int PlayersAtStart { get; set; }
        [Description("Class-D Personnel")]
        public int ClassDPersonnel { get; set; }
        [Description("SCPs")]
        public int SCPs { get; set; }
        public int Scientists { get; set; }
        public int FacilityGuards { get; set; }
        public List<string> Players { get; set; }
    }
}
