using System;

namespace RoundReports
{
    [AttributeUsage(AttributeTargets.Property)]
    public class HideIfDefaultAttribute : Attribute
    {
    }
}