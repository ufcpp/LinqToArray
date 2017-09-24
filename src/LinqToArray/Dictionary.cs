using System;
using System.Collections.Generic;

namespace LinqToArray
{
    public static partial class ArrayExtensions
    {
        /// <summary>
        /// Array optimized
        /// source.ToDictionary(keySelector)
        /// </summary>
        public static CompactDictionary<TKey, TValue, TComparer> ToComapctDictionary<TKey, TValue, TComparer>(this TValue[] source, Func<TValue, TKey> keySelector)
            where TComparer : struct, IEqualityComparer<TKey>
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));

            var d = new CompactDictionary<TKey, TValue, TComparer>(source.Length);

            foreach (var value in source)
            {
                d.AddOrUpdate(keySelector(value), value);
            }

            return d;
        }

        /// <summary>
        /// Array optimized
        /// source.ToDictionary(keySelector, valueSelector)
        /// </summary>
        public static CompactDictionary<TKey, TValue, TComparer> ToComapctDictionary<TSource, TKey, TValue, TComparer>(this TSource[] source, Func<TSource, TKey> keySelector, Func<TSource, TValue> valueSelector)
            where TComparer : struct, IEqualityComparer<TKey>
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));
            if (valueSelector == null) throw new ArgumentNullException(nameof(valueSelector));

            var d = new CompactDictionary<TKey, TValue, TComparer>(source.Length);

            foreach (var x in source)
            {
                d.AddOrUpdate(keySelector(x), valueSelector(x));
            }

            return d;
        }

        /// <summary>
        /// Array optimized
        /// source.ToDictionary(keySelector)
        /// </summary>
        public static CompactDictionary<TKey, TValue, StructEquatableComparer<TKey>> ToComapctDictionary<TKey, TValue>(this TValue[] source, Func<TValue, TKey> keySelector)
            where TKey : IEquatable<TKey>
            => ToComapctDictionary<TKey, TValue, StructEquatableComparer<TKey>>(source, keySelector);

        /// <summary>
        /// Array optimized
        /// source.ToDictionary(keySelector, valueSelector)
        /// </summary>
        public static CompactDictionary<TKey, TValue, StructEquatableComparer<TKey>> ToComapctDictionary<TSource, TKey, TValue>(this TSource[] source, Func<TSource, TKey> keySelector, Func<TSource, TValue> valueSelector)
            where TKey : IEquatable<TKey>
            => ToComapctDictionary<TSource, TKey, TValue, StructEquatableComparer<TKey>>(source, keySelector, valueSelector);

        /// <summary>
        /// Array optimized
        /// source.GroupBy(keySelector)
        /// </summary>
        public static KeyValuePair<TKey, TValue[]>[] GroupByToArray<TKey, TValue, TComparer>(this TValue[] source, Func<TValue, TKey> keySelector)
            where TComparer : struct, IEqualityComparer<TKey>
            => GroupByToArray<TValue, TKey, TValue, TComparer>(source, keySelector, x => x);

        /// <summary>
        /// Array optimized
        /// source.GroupBy(keySelector, valueSelector)
        /// </summary>
        public static unsafe KeyValuePair<TKey, TValue[]>[] GroupByToArray<TSource, TKey, TValue, TComparer>(this TSource[] source, Func<TSource, TKey> keySelector, Func<TSource, TValue> valueSelector)
            where TComparer : struct, IEqualityComparer<TKey>
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));
            if (valueSelector == null) throw new ArgumentNullException(nameof(valueSelector));

            var keyCount = 0;
            var keyToIndex = new CompactDictionary<TKey, int, TComparer>(source.Length);

            var valueCounts = stackalloc int[source.Length];
            var valueToKey = stackalloc int[source.Length];
            var valueIndexes = stackalloc int[source.Length];

            for (int i = 0; i < source.Length; i++)
            {
                valueCounts[i] = 0;
            }

            for (int i = 0; i < source.Length; i++)
            {
                var value = source[i];
                var key = keySelector(value);

                var keyIndex = keyToIndex.GetOrAdd(key, keyCount);

                if (keyIndex == keyCount) keyCount++;

                valueToKey[i] = keyIndex;
                valueIndexes[i] = valueCounts[keyIndex]++;
            }

            var groups = new KeyValuePair<TKey, TValue[]>[keyCount];

            for (int i = 0; i < source.Length; i++)
            {
                var value = source[i];
                var key = keySelector(value);
                var ki = keyToIndex[key];

                if (!(groups[ki].Value is TValue[] values))
                {
                    groups[ki] = new KeyValuePair<TKey, TValue[]>(key, values = new TValue[valueCounts[ki]]);
                }

                values[valueIndexes[i]] = valueSelector(value);
            }

            return groups;
        }

        /// <summary>
        /// Array optimized
        /// source.GroupBy(keySelector)
        /// </summary>
        public static KeyValuePair<TKey, TValue[]>[] GroupByToArray<TKey, TValue>(this TValue[] source, Func<TValue, TKey> keySelector)
            where TKey : IEquatable<TKey>
            => GroupByToArray<TKey, TValue, StructEquatableComparer<TKey>>(source, keySelector);

        /// <summary>
        /// Array optimized
        /// source.GroupBy(keySelector, valueSelector)
        /// </summary>
        public static KeyValuePair<TKey, TValue[]>[] GroupByToArray<TSource, TKey, TValue>(this TSource[] source, Func<TSource, TKey> keySelector, Func<TSource, TValue> valueSelector)
            where TKey : IEquatable<TKey>
            => GroupByToArray<TSource, TKey, TValue, StructEquatableComparer<TKey>>(source, keySelector, valueSelector);
    }
}
