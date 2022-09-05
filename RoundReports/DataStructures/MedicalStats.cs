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
        [Translation(nameof(Translation.MedicalStatsTitle))]
        public string Title => "Medical Statistics";
        public int Order => 5;

        [Translation(nameof(Translation.PainkillersConsumed))]
        public int PainkillersConsumed { get; set; }

        [Translation(nameof(Translation.MedkitsConsumed))]
        public int MedkitsConsumed { get; set; }

        [Translation(nameof(Translation.AdrenalinesConsumed))]
        public int AdrenalinesConsumed { get; set; }

        [Translation(nameof(Translation.Scp500sConsumed))]
        public int SCP500sConsumed { get; set; }
    }
}
