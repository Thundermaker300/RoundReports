namespace RoundReports
{
    using System;
    using System.Collections.Concurrent;

    using Exiled.API.Features.Pools;

    /// <summary>
    /// Pool for storing <see cref="PercentInt"/> objects.
    /// </summary>
    public class PercentIntPool : IPool<PercentInt>
    {
        private readonly ConcurrentQueue<PercentInt> pool = new();

        /// <summary>
        /// Gets the static <see cref="PercentIntPool"/> object.
        /// </summary>
        public static PercentIntPool Pool { get; } = new();

        /// <summary>
        /// Gets a <see cref="PercentInt"/>.
        /// </summary>
        /// <returns>A <see cref="PercentInt"/>.</returns>
        public PercentInt Get()
        {
            if (!pool.TryDequeue(out PercentInt result))
            {
                result = new();
            }

            return result;
        }

        public PercentInt Get(int value, int total, Func<int> updaterMethod)
        {
            if (!pool.TryDequeue(out PercentInt result))
            {
                result = new();
            }

            result.Value = value;
            result.Total = total;
            result.UpdaterMethod = updaterMethod;

            return result;
        }

        /// <inheritdoc/>
        public void Return(PercentInt obj)
        {
            obj.Value = 0;
            obj.Total = 0;
            obj.UpdaterMethod = null;

            pool.Enqueue(obj);
        }
    }
}
