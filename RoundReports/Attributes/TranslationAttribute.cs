using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
