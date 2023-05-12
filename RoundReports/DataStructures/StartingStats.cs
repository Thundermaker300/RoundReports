namespace RoundReports
{
#pragma warning disable SA1600
    using System;
    using System.Collections.Generic;
    using Exiled.API.Features.Pools;
    using PlayerRoles;

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
        public List<string> SCPs { get; set; } // Created in code

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

        public void Setup()
        {
            Players = ListPool<string>.Pool.Get();
        }

        public void Cleanup()
        {
            ListPool<string>.Pool.Return(SCPs);
            ListPool<string>.Pool.Return(Players);
        }

        public void FillOutFinal()
        {
        }
    }
#pragma warning restore SA1600
}
