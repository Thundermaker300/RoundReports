namespace RoundReports
{
    using System;

    /// <summary>
    /// Tells the reporter to not show a stat if it matches its default value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class HideIfDefaultAttribute : Attribute
    {
    }
}