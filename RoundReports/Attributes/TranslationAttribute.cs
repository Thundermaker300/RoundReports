namespace RoundReports
{
    using System;
    using System.Reflection;

    /// <summary>
    /// Generate a stat title from translations.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class TranslationAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TranslationAttribute"/> class.
        /// </summary>
        /// <param name="translationName">The key to use from the translation file.</param>
        public TranslationAttribute(string translationName)
        {
            KeyName = translationName;
        }

        /// <summary>
        /// Gets the key that is being used from translation.
        /// </summary>
        public string KeyName { get; }

        /// <summary>
        /// Gets the text that will be displayed as the stat.
        /// </summary>
        public string Text
        {
            get
            {
                PropertyInfo info = typeof(Translation).GetProperty(KeyName);
                return info.GetValue(MainPlugin.Translations).ToString();
            }
        }
    }
}
