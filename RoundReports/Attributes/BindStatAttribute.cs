using System;

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
