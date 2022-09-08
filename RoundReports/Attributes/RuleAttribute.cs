using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoundReports
{
    [Flags]
    public enum Rule
    {
        None = 0,
        Alphabetical = 1, // TODO: Make this work (somehow)
        CommaSeparatedList = 2,
    }

    public class RuleAttribute : Attribute
    {
        public Rule Rule { get; }
        public RuleAttribute(Rule r)
        {
            Rule = r;
        }
    }
}
