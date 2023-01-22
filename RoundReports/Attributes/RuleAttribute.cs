namespace RoundReports
{
    using System;

    /// <summary>
    /// Provides a set of rules for a stat's display on the report.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class RuleAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RuleAttribute"/> class.
        /// </summary>
        /// <param name="r">The rule to enforce on the stat.</param>
        public RuleAttribute(Rule r)
        {
            Rule = r;
        }

        /// <summary>
        /// Gets the rules for the stat.
        /// </summary>
        public Rule Rule { get; }
    }
}
