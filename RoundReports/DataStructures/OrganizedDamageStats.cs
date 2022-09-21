﻿using Exiled.API.Enums;
using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public Dictionary<Player, int> DamageByPlayer { get; set; } = new(0);

        [Translation(nameof(Translation.DamageByType))]
        [BindStat(StatType.DamageByType)]
        public Dictionary<DamageType, int> DamageByType { get; set; } = new(0);
    }
}
