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

        /// <summary>
        /// Can be used on lists and dictionaries to sort them alphabetically. Dictionaries will sort by key.
        /// </summary>
        Alphabetical = 1, // note: this currently does not work lol

        /// <summary>
        /// Can be used on lists to turn them into a comma separated list, rather than the default setting.
        /// </summary>
        CommaSeparatedList = 2,
    }

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
