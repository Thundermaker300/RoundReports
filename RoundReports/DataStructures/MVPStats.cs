using Exiled.API.Features;
using NorthwoodLib.Pools;
using System.Collections.Generic;

namespace RoundReports
{
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
        public Dictionary<Player, int> HumanPoints { get; set; }

        [Translation(nameof(Translation.SCPPoints))]
        [BindStat(StatType.SCPPoints)]
        public Dictionary<Player, int> SCPPoints { get; set; }

        [BindStat(StatType.PointLogs)]
        public List<string> PointLogs { get; set; }

        public void Setup()
        {
            PointLogs = ListPool<string>.Shared.Rent();
        }

        public void Cleanup()
        {
            ListPool<string>.Shared.Return(PointLogs);
        }
    }
}
