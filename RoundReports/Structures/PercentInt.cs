namespace RoundReports
{
    using System;

    /// <summary>
    /// Used in place of integer stats to show a percentage next to them.
    /// </summary>
    public class PercentInt
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PercentInt"/> class.
        /// </summary>
        public PercentInt()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PercentInt"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="total">The total.</param>
        /// <param name="updaterMethod">The method to run at the end of the round.</param>
        public PercentInt(int value, int total, Func<int> updaterMethod = null)
        {
            Value = value;
            Total = total;
            UpdaterMethod = updaterMethod;
        }

        /// <summary>
        /// Gets or sets a value indicating the current value.
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the total.
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// Gets the percent calculated from the Value and the Total.
        /// </summary>
        public float Percent => (float)Math.Round(((float)Value / Total) * 100, 1);

        /// <summary>
        /// Gets or sets a method to update the total at the end of a round.
        /// </summary>
        public Func<int> UpdaterMethod { get; set; }

        /// <summary>
        /// Updates value and total.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <param name="total">The new total.</param>
        public void Update(int value, int total)
        {
            Value = value;
            Total = total;
        }

        /// <summary>
        /// Increments the Value and sets the Total.
        /// </summary>
        /// <param name="valueIncrement">The value to increment the value by.</param>
        /// <param name="newTotal">The new total.</param>
        public void IncrementValue(int valueIncrement, int? newTotal = null)
        {
            Value += valueIncrement;

            if (newTotal.HasValue)
                Total = newTotal.Value;
        }
    }
}
