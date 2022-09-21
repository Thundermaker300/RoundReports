using System;
using System.Collections.Generic;

namespace RoundReports
{
    public class StartingStats : IReportStat
    {
        [Translation(nameof(Translation.StartingTitle))]
        public string Title => "Starting Statistics";
        public int Order => 3;

        [Translation(nameof(Translation.StartTime))]
        [BindStat(StatType.StartTime)]
        public DateTime StartTime { get; set; }

        [Translation(nameof(Translation.PlayersAtStart))]
        [BindStat(StatType.StartPlayers)]
        public int PlayersAtStart { get; set; }

        [Rule(Rule.Alphabetical | Rule.CommaSeparatedList)]
        [Translation(nameof(Translation.Scps))]
        [BindStat(StatType.StartSCP)]
        public List<RoleType> SCPs { get; set; }

        [Translation(nameof(Translation.ClassDPersonnel))]
        [BindStat(StatType.StartClassD)]
        public int ClassDPersonnel { get; set; }

        [Translation(nameof(Translation.Scientists))]
        [BindStat(StatType.StartScientist)]
        public int Scientists { get; set; }

        [Translation(nameof(Translation.FacilityGuards))]
        [BindStat(StatType.StartFacilityGuard)]
        public int FacilityGuards { get; set; }

        [Translation(nameof(Translation.Players))]
        [BindStat(StatType.StartPlayers)]
        public List<string> Players { get; set; }
    }
}
