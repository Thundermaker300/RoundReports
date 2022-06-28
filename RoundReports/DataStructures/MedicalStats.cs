using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoundReports
{
    public struct MedicalStats : IReportStat
    {
        public string Title => "Medical Statistics";
        public int PillsUsed { get; set; }
        public int MedkitsUsed { get; set; }
        public int AdrenalinesUsed { get; set; }
        public int SCP500sUsed { get; set; }
    }
}
