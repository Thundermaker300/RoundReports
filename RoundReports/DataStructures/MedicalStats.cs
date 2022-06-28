using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoundReports
{
    public class MedicalStats : IReportStat
    {
        public string Title => "Medical Statistics";
        public int Order => 5;
        public int PillsConsumed { get; set; }
        public int MedkitsConsumed { get; set; }
        public int AdrenalinesConsumed { get; set; }
        [Description("SCP-500s Consumed")]
        public int SCP500sConsumed { get; set; }
    }
}
