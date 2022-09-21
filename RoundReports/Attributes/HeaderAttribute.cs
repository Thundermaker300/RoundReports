using System;
using System.Reflection;

namespace RoundReports
{
    [AttributeUsage(AttributeTargets.Property)]
    public class HeaderAttribute : Attribute
    {
        public string KeyName { get; }
        public string Header
        {
            get
            {
                PropertyInfo info = typeof(Translation).GetProperty(KeyName);
                return info.GetValue(MainPlugin.Translations).ToString();
            }
        }
        public HeaderAttribute(string keyName)
        {
            KeyName = keyName;
        }
    }
}
