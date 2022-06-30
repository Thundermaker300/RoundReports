using Exiled.API.Enums;
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
        public string Title => "Damage Dealt";
        public int Order => 51;

        public int TotalDamage { get; set; }
        public int PlayerDamage { get; set; }
        public Dictionary<Player, int> DamageByPlayer { get; set; } = new(0);
        public Dictionary<DamageType, int> DamageByType { get; set; } = new(0);
    }
}
