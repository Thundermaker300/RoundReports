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
        [Translation(nameof(Translation.ScpItemTitle))]
        public string Title => "SCP Item Statistics";
        public int Order => 6;

        [Translation(nameof(Translation.Scp018Thrown))]
        public int Scp018Thrown { get; set; }

        [Translation(nameof(Translation.Scp207Drank))]
        public int Scp207Drank { get; set; }

        [Translation(nameof(Translation.Scp268Uses))]
        public int Scp268Uses { get; set; }

        [Translation(nameof(Translation.Scp1853Uses))]
        public int Scp1853Uses { get; set; }
    }
}
