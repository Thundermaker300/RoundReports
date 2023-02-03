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

        [Translation(nameof(Translation.AverageDamagePerPlayer))]
        [BindStat(StatType.AverageDamagePerPlayer)]
        public float AverageDamagePerPlayer { get; set; }

        [Translation(nameof(Translation.DamageByPlayer))]
        [BindStat(StatType.DamageByPlayer)]
        public Dictionary<Player, PercentInt> DamageByPlayer { get; set; }

        [Translation(nameof(Translation.DamageByType))]
        [BindStat(StatType.DamageByType)]
        public Dictionary<DamageType, PercentInt> DamageByType { get; set; }

        public void Setup()
        {
            DamageByPlayer = DictionaryPool<Player, PercentInt>.Pool.Get();
            DamageByType = DictionaryPool<DamageType, PercentInt>.Pool.Get();
        }

        public void Cleanup()
        {
            foreach (KeyValuePair<Player, PercentInt> kvp in DamageByPlayer)
                PercentIntPool.Pool.Return(kvp.Value);

            foreach (KeyValuePair<DamageType, PercentInt> kvp in DamageByType)
                PercentIntPool.Pool.Return(kvp.Value);

            DictionaryPool<Player, PercentInt>.Pool.Return(DamageByPlayer);
            DictionaryPool<DamageType, PercentInt>.Pool.Return(DamageByType);
        }

        public void FillOutFinal()
        {
            if (DamageByPlayer.Count > 0 && PlayerDamage > 0)
            {
                AverageDamagePerPlayer = PlayerDamage / DamageByPlayer.Count;
            }
        }
    }
#pragma warning restore SA1600
}
