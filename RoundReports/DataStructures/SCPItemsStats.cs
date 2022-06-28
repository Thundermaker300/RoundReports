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
    }
}
