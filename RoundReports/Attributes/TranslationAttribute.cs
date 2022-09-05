using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoundReports
{
    [AttributeUsage(AttributeTargets.Property)]
    public class TranslationAttribute : Attribute
    {
        public string KeyName { get; }

        public TranslationAttribute(string translationName)
        {
            KeyName = translationName;
        }
    }
}
