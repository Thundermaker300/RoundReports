namespace RoundReports
{
#pragma warning disable SA1600
    using System.Collections.Generic;
    using Exiled.API.Features;
    using Exiled.API.Features.Pools;

    public class MVPStats : IReportStat
    {
        [Translation(nameof(Translation.MVPTitle))]
        public string Title => "MVPs";

        public int Order => 1;

        [Translation(nameof(Translation.HumanMVP))]
        [BindStat(StatType.HumanMVP)]
        public string HumanMVP { get; set; }

        [Translation(nameof(Translation.SCPMVP))]
        [BindStat(StatType.SCPMVP)]
        public string SCPMVP { get; set; }

        [Translation(nameof(Translation.HumanPoints))]
        [BindStat(StatType.HumanPoints)]
        public Dictionary<Player, int> HumanPoints { get; set; } // Assigned in code

        [Translation(nameof(Translation.SCPPoints))]
        [BindStat(StatType.SCPPoints)]
        public Dictionary<Player, int> SCPPoints { get; set; } // Assigned in code

        [BindStat(StatType.PointLogs)]
        public List<string> PointLogs { get; set; }

        public void Setup()
        {
            PointLogs = ListPool<string>.Pool.Get();
            HumanMVP = MainPlugin.Translations.NoData;
            SCPMVP = MainPlugin.Translations.NoData;
        }

        public void Cleanup()
        {
            ListPool<string>.Pool.Return(PointLogs);
        }

        public void FillOutFinal()
        {
        }
    }
#pragma warning restore SA1600
}
