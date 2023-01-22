namespace RoundReports
{
    using System;
    using System.Reflection;

    /// <summary>
    /// Generate a header using translations.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class HeaderAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HeaderAttribute"/> class.
        /// </summary>
        /// <param name="keyName">The key to use from the translation file.</param>
        public HeaderAttribute(string keyName)
        {
            KeyName = keyName;
        }

        /// <summary>
        /// Gets the key that is being used from translation.
        /// </summary>
        public string KeyName { get; }

        /// <summary>
        /// Gets the text that will be displayed as a header.
        /// </summary>
        public string Header
        {
            get
            {
                PropertyInfo info = typeof(Translation).GetProperty(KeyName);
                return info.GetValue(MainPlugin.Translations).ToString();
            }
        }
    }
}
