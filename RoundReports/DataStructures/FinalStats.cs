using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoundReports
{
    public class FinalStats : IReportStat
    {
        public string Title => "Final Statistics";
        public int Order => 1;
        public string WinningTeam { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan RoundTime { get; set; }
        public int TotalDeaths { get; set; }
        public int TotalKills { get; set; }
        [Description("SCP Kills")]
        public int SCPKills { get; set; }
        [Description("Class-D Kills")]
        public int DClassKills { get; set; }
        public int ScientistKills { get; set; }
        [Description("MTF Kills")]
        public int MTFKills { get; set; }
        public int ChaosKills { get; set; }
        public int TutorialKills { get; set; }
        public List<string> SurvivingPlayers { get; set; }
    }
}
