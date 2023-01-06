using System.Collections.Concurrent;
using System.Collections.Generic;
using NorthwoodLib.Pools;

namespace RoundReports
{
    public class DictPool<TKey, TValue> : IPool<Dictionary<TKey, TValue>>
    {
        private readonly ConcurrentQueue<Dictionary<TKey, TValue>> _pool = new();
        public static DictPool<TKey, TValue> Shared { get; } = new();
        
        public Dictionary<TKey, TValue> Rent()
        {
            Dictionary<TKey, TValue> dict;
            if (this._pool.TryDequeue(out dict))
            {
                return dict;
            }
            return new Dictionary<TKey, TValue>();
        }

        public void Return(Dictionary<TKey, TValue> obj)
        {
            obj.Clear();
            _pool.Enqueue(obj);
        }
    }
}
