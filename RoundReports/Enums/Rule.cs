using System;

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
}
