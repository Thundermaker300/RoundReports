using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoundReports
{
    public struct MedicalStats : IReportStat
    {
        public string Title => "Medical Statistics";

        [Description("Pills Consumed")]
        public int PillsUsed { get; set; }

        [Description("Medkits Consumed")]
        public int MedkitsUsed { get; set; }

        [Description("Adrenalines Consumed")]
        public int AdrenalinesUsed { get; set; }

        [Description("SCP-500s Consumed")]
        public int SCP500sUsed { get; set; }
    }
}
