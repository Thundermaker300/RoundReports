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
        [Translation(nameof(Translation.StartingTitle))]
        public string Title => "Starting Statistics";
        public int Order => 2;

        [Translation(nameof(Translation.StartTime))]
        public DateTime StartTime { get; set; }

        [Translation(nameof(Translation.PlayersAtStart))]
        public int PlayersAtStart { get; set; }

        [Rule(Rule.Alphabetical | Rule.CommaSeparatedList)]
        [Translation(nameof(Translation.Scps))]
        public List<RoleType> SCPs { get; set; }

        [Translation(nameof(Translation.ClassDPersonnel))]
        public int ClassDPersonnel { get; set; }

        [Translation(nameof(Translation.Scientists))]
        public int Scientists { get; set; }

        [Translation(nameof(Translation.FacilityGuards))]
        public int FacilityGuards { get; set; }

        [Translation(nameof(Translation.Players))]
        public List<string> Players { get; set; }
    }
}
