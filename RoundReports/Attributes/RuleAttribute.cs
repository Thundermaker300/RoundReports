using System;

namespace RoundReports
{
    [AttributeUsage(AttributeTargets.Property)]
    public class RuleAttribute : Attribute
    {
        public Rule Rule { get; }
        public RuleAttribute(Rule r)
        {
            Rule = r;
        }
    }
}
