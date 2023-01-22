namespace RoundReports
{
    using System;

    /// <summary>
    /// Attribute used to connect a property to its corresponding <see cref="StatType"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class BindStatAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BindStatAttribute"/> class.
        /// </summary>
        /// <param name="type">The <see cref="StatType"/> to bind.</param>
        public BindStatAttribute(StatType type)
        {
            Type = type;
        }

        /// <summary>
        /// Gets the <see cref="StatType"/> that this property is connected to.
        /// </summary>
        public StatType Type { get; }
    }
}
