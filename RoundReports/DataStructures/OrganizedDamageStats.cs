using Exiled.API.Enums;
using Exiled.API.Features;
using System.Collections.Generic;

namespace RoundReports
{
    public class OrganizedDamageStats : IReportStat
    {
        [Translation(nameof(Translation.OrganizedDamageTitle))]
        public string Title => "Damage Dealt";
        public int Order => 51;

        [Translation(nameof(Translation.TotalDamage))]
        [BindStat(StatType.TotalDamage)]
        public int TotalDamage { get; set; }

        [Translation(nameof(Translation.PlayerDamage))]
        [BindStat(StatType.PlayerDamage)]
        public int PlayerDamage { get; set; }

        [Translation(nameof(Translation.DamageByPlayer))]
        [BindStat(StatType.DamageByPlayer)]
        public Dictionary<Player, int> DamageByPlayer { get; set; }

        [Translation(nameof(Translation.DamageByType))]
        [BindStat(StatType.DamageByType)]
        public Dictionary<DamageType, int> DamageByType { get; set; }

        public void Setup()
        {
            DamageByPlayer = DictPool<Player, int>.Shared.Rent();
            DamageByType = DictPool<DamageType, int>.Shared.Rent();
        }

        public void Cleanup()
        {
            DictPool<Player, int>.Shared.Return(DamageByPlayer);
            DictPool<DamageType, int>.Shared.Return(DamageByType);
        }
    }
}
