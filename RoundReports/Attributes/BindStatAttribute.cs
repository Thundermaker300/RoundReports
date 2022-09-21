using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoundReports
{
    [AttributeUsage(AttributeTargets.Property)]
    public class BindStatAttribute : Attribute
    {
        public StatType Type { get; }

        public BindStatAttribute(StatType type)
        {
            Type = type;
        }
    }
}
