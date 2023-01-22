namespace RoundReports
{
#pragma warning disable SA1600
    using System.Collections.Generic;
    using Exiled.API.Enums;
    using Exiled.API.Features;
    using Exiled.API.Features.Pools;

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
            DamageByPlayer = DictionaryPool<Player, int>.Pool.Get();
            DamageByType = DictionaryPool<DamageType, int>.Pool.Get();
        }

        public void Cleanup()
        {
            DictionaryPool<Player, int>.Pool.Return(DamageByPlayer);
            DictionaryPool<DamageType, int>.Pool.Return(DamageByType);
        }
    }
#pragma warning restore SA1600
}
