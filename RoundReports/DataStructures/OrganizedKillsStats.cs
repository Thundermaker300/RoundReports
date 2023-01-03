using Exiled.API.Enums;
using Exiled.API.Features;
using NorthwoodLib.Pools;
using System.Collections.Generic;

namespace RoundReports
{
    public class OrganizedKillsStats : IReportStat
    {
        [Translation(nameof(Translation.OrganizedKillsTitle))]
        public string Title => "Kills";
        public int Order => 50;

        [Translation(nameof(Translation.KillsByPlayer))]
        [BindStat(StatType.KillsByPlayer)]
        public Dictionary<Player, int> KillsByPlayer { get; set; }

        [Translation(nameof(Translation.KillsbyType))]
        [BindStat(StatType.KillsByType)]
        public Dictionary<DamageType, int> KillsByType { get; set; }

        [Translation(nameof(Translation.PlayerKills))]
        [BindStat(StatType.PlayerKills)]
        public List<string> PlayerKills { get; set; }

        public void Setup()
        {
            PlayerKills = ListPool<string>.Shared.Rent();
            KillsByPlayer = new();
            KillsByType = new();
        }

        public void Cleanup()
        {
            ListPool<string>.Shared.Return(PlayerKills);
        }
    }
}
