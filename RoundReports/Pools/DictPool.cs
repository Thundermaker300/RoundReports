using System.Collections.Concurrent;
using System.Collections.Generic;
using Exiled.API.Features;
using NorthwoodLib.Pools;

namespace RoundReports
{
    public class DictPool<TKey, TValue> : IPool<Dictionary<TKey, TValue>>
    {
        private readonly ConcurrentQueue<Dictionary<TKey, TValue>> _pool = new();
        public static DictPool<TKey, TValue> Shared { get; } = new();
        
        public Dictionary<TKey, TValue> Rent()
        {
            Log.Debug($"Renting dict pool {typeof(TKey).Name},{typeof(TValue).Name}.");
            if (this._pool.TryDequeue(out Dictionary<TKey, TValue> dict))
            {
                return dict;
            }

            return new Dictionary<TKey, TValue>();
        }

        public void Return(Dictionary<TKey, TValue> obj)
        {
            Log.Debug($"Returning dict pool {typeof(TKey).Name},{typeof(TValue).Name}.");
            obj.Clear();
            _pool.Enqueue(obj);
        }
    }
}
