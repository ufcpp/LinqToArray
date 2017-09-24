using System.Collections.Generic;
using System.Linq;

namespace LinqToArrayUnitTest
{
    class ArrayComparer<T> : IEqualityComparer<T[]>
    {
        public static readonly ArrayComparer<T> Default = new ArrayComparer<T>();

        public bool Equals(T[] x, T[] y) => x.SequenceEqual(y);
        public int GetHashCode(T[] obj) => obj.Aggregate(0, (x, item) => x * 2345643 ^ item.GetHashCode());
    }

    class DictionaryComparer<TKey, TValue> : IEqualityComparer<IReadOnlyDictionary<TKey, TValue>>
    {
        public static readonly DictionaryComparer<TKey, TValue> Default = new DictionaryComparer<TKey, TValue>();

        private IEqualityComparer<TValue> _valueComparer;

        public DictionaryComparer() : this(EqualityComparer<TValue>.Default) { }
        public DictionaryComparer(IEqualityComparer<TValue> valueComparer) => _valueComparer = valueComparer;

        public bool Equals(IReadOnlyDictionary<TKey, TValue> x, IReadOnlyDictionary<TKey, TValue> y)
        {
            if (x.Keys.Except(y.Keys).Any()) return false;
            if (y.Keys.Except(x.Keys).Any()) return false;

            foreach (var item in x)
            {
                var vx = item.Value;
                if (!y.TryGetValue(item.Key, out var vy)) return false;
                if (!_valueComparer.Equals(vx, vy)) return false;
            }
            return true;
        }

        public int GetHashCode(IReadOnlyDictionary<TKey, TValue> obj) => obj.Aggregate(0, (x, item) => x * 2345643 ^ item.Key.GetHashCode() * 123432765 ^ item.Value.GetHashCode());
    }
}
