namespace RoundReports
{
    using System;
    using System.Collections.Concurrent;

    using Exiled.API.Features.Pools;

    public class PercentIntPool : IPool<PercentInt>
    {
        private readonly ConcurrentQueue<PercentInt> pool = new();

        public static PercentIntPool Pool { get; } = new();

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

        public void Return(PercentInt obj)
        {
            obj.Value = 0;
            obj.Total = 0;
        }
    }
}
