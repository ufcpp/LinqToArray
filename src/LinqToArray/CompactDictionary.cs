using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LinqToArray
{
    public struct CompactDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
        where TKey : IEquatable<TKey>
    {
        private CompactDictionary<TKey, TValue, StructEquatableComparer<TKey>> _inner;

        public TValue this[TKey key] => ((IReadOnlyDictionary<TKey, TValue>)_inner)[key];
        public IEnumerable<TKey> Keys => ((IReadOnlyDictionary<TKey, TValue>)_inner).Keys;
        public IEnumerable<TValue> Values => ((IReadOnlyDictionary<TKey, TValue>)_inner).Values;
        public int Count => ((IReadOnlyDictionary<TKey, TValue>)_inner).Count;
        public bool ContainsKey(TKey key) => ((IReadOnlyDictionary<TKey, TValue>)_inner).ContainsKey(key);
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => ((IReadOnlyDictionary<TKey, TValue>)_inner).GetEnumerator();
        public bool TryGetValue(TKey key, out TValue value) => ((IReadOnlyDictionary<TKey, TValue>)_inner).TryGetValue(key, out value);
        IEnumerator IEnumerable.GetEnumerator() => ((IReadOnlyDictionary<TKey, TValue>)_inner).GetEnumerator();
    }

    public struct CompactDictionary<TKey, TValue, TComparer> : IReadOnlyDictionary<TKey, TValue>
        where TComparer : struct, IEqualityComparer<TKey>
    {
        internal struct Bucket
        {
            public bool HasValue;
            public TKey Key;
            public TValue Value;

            internal KeyValuePair<TKey, TValue> ToKeyValuePair() => new KeyValuePair<TKey, TValue>(Key, Value);
        }
        private const int Skip = 655883; // a big prime

        private Bucket[] _buckets;

        public bool IsNull => _buckets == null;

        public CompactDictionary(int capacity)
        {
            var initialCapacity = capacity * 2;
            capacity = ArrayExtensions.PowerOf2(initialCapacity);
            _buckets = new Bucket[capacity];
        }

        public void AddOrUpdate(TKey key, TValue value)
        {
            var mask = _buckets.Length - 1;

            var hash = default(TComparer).GetHashCode(key) & mask;

            while (true)
            {
                ref var b = ref _buckets[hash];

                if (!b.HasValue)
                {
                    b.Key = key;
                    b.Value = value;
                    b.HasValue = true;
                    break;
                }

                hash = (hash + Skip) % mask;
            }
        }

        public TValue GetOrAdd(TKey key, TValue value)
        {
            var mask = _buckets.Length - 1;

            var hash = default(TComparer).GetHashCode(key) & mask;

            while (true)
            {
                ref var b = ref _buckets[hash];

                if (!b.HasValue)
                {
                    b.Key = key;
                    b.Value = value;
                    b.HasValue = true;
                    return value;
                }
                else if (default(TComparer).Equals(b.Key, key)) return b.Value;

                hash = (hash + Skip) % mask;
            }
        }

        public CompactDictionary(IEnumerable<KeyValuePair<TKey, TValue>> values)
        {
            var initialCapacity = values.Count() * 2;
            var capacity = ArrayExtensions.PowerOf2(initialCapacity);

            _buckets = new Bucket[capacity];
            var mask = capacity - 1;

            foreach (var x in values)
            {
                var hash = default(TComparer).GetHashCode(x.Key) & mask;

                while (true)
                {
                    ref var b = ref _buckets[hash];

                    if (!b.HasValue)
                    {
                        b.HasValue = true;
                        b.Key = x.Key;
                        b.Value = x.Value;
                        break;
                    }

                    hash = (hash + Skip) % mask;
                }
            }
        }

        public TValue this[TKey key] => TryGetValue(key, out var value) ? value : throw new KeyNotFoundException(key.ToString());

        public bool TryGetValue(TKey key, out TValue value)
        {
            var mask = _buckets.Length - 1;
            var hash = default(TComparer).GetHashCode(key) & mask;

            while (true)
            {
                ref var b = ref _buckets[hash];

                if (!b.HasValue)
                {
                    value = default(TValue);
                    return false;
                }
                else if (default(TComparer).Equals(b.Key, key))
                {
                    value = b.Value;
                    return true;
                }

                hash = (hash + Skip) % mask;
            }
        }

        public Enumerator GetEnumerator() => new Enumerator(_buckets);
        public KeyEnumerable Keys => new KeyEnumerable(_buckets);
        public ValueEnumerable Values => new ValueEnumerable(_buckets);

        public bool ContainsKey(TKey key) => TryGetValue(key, out _);
        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;
        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;
        public int Count => _buckets.Count(b => b.HasValue);

        public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>
        {
            private int _index;
            private Bucket[] _buckets;

            internal Enumerator(Bucket[] buckets)
            {
                _index = -1;
                _buckets = buckets;
            }

            public KeyValuePair<TKey, TValue> Current => _buckets[_index].ToKeyValuePair();

            public bool MoveNext()
            {
                while (true)
                {
                    _index++;
                    if (_index >= _buckets.Length) return false;
                    if (_buckets[_index].HasValue) return true;
                }
            }

            object IEnumerator.Current => Current;
            public void Dispose() { }
            public void Reset() => throw new NotImplementedException();
        }

        public struct KeyEnumerable : IEnumerable<TKey>
        {
            private Bucket[] _buckets;
            internal KeyEnumerable(Bucket[] buckets) => _buckets = buckets;
            public KeyEnumerator GetEnumerator() => new KeyEnumerator(_buckets);
            IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator() => GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        public struct KeyEnumerator : IEnumerator<TKey>
        {
            private int _index;
            private Bucket[] _buckets;

            internal KeyEnumerator(Bucket[] buckets)
            {
                _index = -1;
                _buckets = buckets;
            }

            public TKey Current => _buckets[_index].Key;

            public bool MoveNext()
            {
                while (true)
                {
                    _index++;
                    if (_index >= _buckets.Length) return false;
                    if (_buckets[_index].HasValue) return true;
                }
            }

            object IEnumerator.Current => Current;
            public void Dispose() { }
            public void Reset() => throw new NotImplementedException();
        }

        public struct ValueEnumerable : IEnumerable<TValue>
        {
            private Bucket[] _buckets;
            internal ValueEnumerable(Bucket[] buckets) => _buckets = buckets;
            public ValueEnumerator GetEnumerator() => new ValueEnumerator(_buckets);
            IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator() => GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        public struct ValueEnumerator : IEnumerator<TValue>
        {
            private int _index;
            private Bucket[] _buckets;

            internal ValueEnumerator(Bucket[] buckets)
            {
                _index = -1;
                _buckets = buckets;
            }

            public TValue Current => _buckets[_index].Value;

            public bool MoveNext()
            {
                while (true)
                {
                    _index++;
                    if (_index >= _buckets.Length) return false;
                    if (_buckets[_index].HasValue) return true;
                }
            }

            object IEnumerator.Current => Current;
            public void Dispose() { }
            public void Reset() => throw new NotImplementedException();
        }
    }
}
