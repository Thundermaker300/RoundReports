using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoundReports
{
    public class FinalStats : IReportStat
    {
        [Translation(nameof(Translation.FinalStatsTitle))]
        public string Title => "Final Statistics";
        public int Order => 2;

        [Header(nameof(Translation.EndofRoundSummary))]
        [Translation(nameof(Translation.WinningTeam))]
        public string WinningTeam { get; set; }

        [Translation(nameof(Translation.EndTime))]
        public DateTime EndTime { get; set; }

        [Translation(nameof(Translation.RoundTime))]
        public TimeSpan RoundTime { get; set; }

        [Translation(nameof(Translation.TotalDeaths))]
        public int TotalDeaths { get; set; }

        [Translation(nameof(Translation.TotalKills))]
        public int TotalKills { get; set; }

        [Translation(nameof(Translation.ScpKills))]
        public int SCPKills { get; set; }

        [Translation(nameof(Translation.DClassKills))]
        public int DClassKills { get; set; }

        [Translation(nameof(Translation.ScientistKills))]
        public int ScientistKills { get; set; }

        [Translation(nameof(Translation.MtfKills))]
        public int MTFKills { get; set; }

        [Translation(nameof(Translation.ChaosKills))]
        public int ChaosKills { get; set; }

        [HideIfDefault]
        [Translation(nameof(Translation.SerpentsHandKills))]
        public int SerpentsHandKills { get; set; }

        [HideIfDefault]
        [Translation(nameof(Translation.UiuKills))]
        public int UIUKills { get; set; }

        [HideIfDefault]
        [Translation(nameof(Translation.TutorialKills))]
        public int TutorialKills { get; set; }

        [Translation(nameof(Translation.SurvivingPlayers))]
        public List<string> SurvivingPlayers { get; set; } = new();

        [Header(nameof(Translation.WarheadStatsTitle))]
        [Translation(nameof(Translation.ButtonUnlocked))]
        public bool ButtonUnlocked { get; set; } = false;

        [Translation(nameof(Translation.ButtonUnlocker))]
        public Player ButtonUnlocker { get; set; }

        [Translation(nameof(Translation.FirstActivator))]
        public Player FirstActivator { get; set; }

        [Translation(nameof(Translation.Detonated))]
        public bool Detonated { get; set; } = false;

        [Translation(nameof(Translation.DetonationTime))]
        public DateTime DetonationTime { get; set; } = DateTime.MinValue;

        [Header(nameof(Translation.DoorStatsTitle))]
        [Translation(nameof(Translation.DoorsOpened))]
        public int DoorsOpened { get; set; }

        [Translation(nameof(Translation.DoorsClosed))]
        public int DoorsClosed { get; set; }

        [Translation(nameof(Translation.DoorsDestroyed))]
        public int DoorsDestroyed { get; set; }

        [Translation(nameof(Translation.PlayerDoorsOpened))]
        public Dictionary<Player, int> PlayerDoorsOpened { get; set; } = new(0);

        [Translation(nameof(Translation.PlayerDoorsClosed))]
        public Dictionary<Player, int> PlayerDoorsClosed { get; set; } = new(0);
    }
}
