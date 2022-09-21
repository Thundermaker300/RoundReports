using System;
using System.Reflection;

namespace RoundReports
{
    [AttributeUsage(AttributeTargets.Property)]
    public class TranslationAttribute : Attribute
    {
        public string KeyName { get; }

        public string Text
        {
            get
            {
                PropertyInfo info = typeof(Translation).GetProperty(KeyName);
                return info.GetValue(MainPlugin.Translations).ToString();
            }
        }

        public TranslationAttribute(string translationName)
        {
            KeyName = translationName;
        }
    }
}
