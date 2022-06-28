using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoundReports
{
    public class SCPItemStats : IReportStat
    {
        public string Title => "SCP Item Statistics";
        public int Order => 6;
        [Description("SCP-207's Drank")]
        public int Scp207Drank { get; set; }
        [Description("SCP-268 Uses")]
        public int Scp268Uses { get; set; }
        [Description("SCP-1853 Uses")]
        public int Scp1853Uses { get; set; }
    }
}
