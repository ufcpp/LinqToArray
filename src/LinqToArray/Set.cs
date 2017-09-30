using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace LinqToArray
{
    public static partial class ArrayExtensions
    {
#if !NET35
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        internal static int PowerOf2(int x)
        {
            var p = 1;
            while (p < x) p <<= 1;
            return p;
        }

        private static unsafe T[] ToArray<T>(T[] items, int* indexes, int count)
        {
            var results = new T[count];
            for (int i = 0; i < results.Length; i++)
            {
                results[i] = items[indexes[i]];
            }
            return results;
        }

        /// <summary>
        /// Array optimized
        /// source.Distinct().ToArray()
        /// </summary>
        public static unsafe T[] DistinctToArray<T, TComp>(this T[] source)
            where TComp : struct, IEqualityComparer<T>
        {
            var len = PowerOf2(source.Length * 2);
            var buckets = stackalloc int[len];
            var set = new StackHashSet<T, TComp>(source, buckets, len);

            var resultIndexes = stackalloc int[source.Length];
            var count = 0;

            for (int i = 0; i < source.Length; i++)
            {
                if (!set.Add(source[i], i))
                {
                    resultIndexes[count++] = i;
                }
            }

            return ToArray(source, resultIndexes, count);
        }

        /// <summary>
        /// Array optimized
        /// source.Distinct().ToArray()
        /// </summary>
        public static unsafe T[] DistinctToArray<T>(this T[] source)
            where T : IEquatable<T>
            => DistinctToArray<T, StructEquatableComparer<T>>(source);

        /// <summary>
        /// Array optimized
        /// first.Except(second).ToArray()
        /// </summary>
        public static unsafe T[] ExceptToArray<T, TComp>(this T[] first, T[] second)
            where TComp : struct, IEqualityComparer<T>
        {
            var len = PowerOf2(second.Length * 2);
            var buckets = stackalloc int[len];
            var set = new StackHashSet<T, TComp>(second, buckets, len);

            set.MakeHashtTable();

            var resultIndexes = stackalloc int[first.Length];
            var count = 0;

            for (int i = 0; i < first.Length; i++)
            {
                if (!set.Contains(first[i]))
                {
                    resultIndexes[count++] = i;
                }
            }

            return ToArray(first, resultIndexes, count);
        }

        /// <summary>
        /// Array optimized
        /// first.Intersect(second).ToArray()
        /// </summary>
        public static unsafe T[] ExceptToArray<T>(this T[] first, T[] second)
            where T : IEquatable<T>
            => ExceptToArray<T, StructEquatableComparer<T>>(first, second);

        /// <summary>
        /// Array optimized
        /// first.Intersect(second).ToArray()
        /// </summary>
        public static unsafe T[] IntersectToArray<T, TComp>(this T[] first, T[] second)
            where TComp : struct, IEqualityComparer<T>
        {
            var len = PowerOf2(second.Length * 2);
            var buckets = stackalloc int[len];
            var set = new StackHashSet<T, TComp>(second, buckets, len);

            set.MakeHashtTable();

            var resultIndexes = stackalloc int[first.Length];
            var count = 0;

            for (int i = 0; i < first.Length; i++)
            {
                if (set.Contains(first[i]))
                {
                    resultIndexes[count++] = i;
                }
            }

            return ToArray(first, resultIndexes, count);
        }

        /// <summary>
        /// Array optimized
        /// first.Intersect(second).ToArray()
        /// </summary>
        public static unsafe T[] IntersectToArray<T>(this T[] first, T[] second)
            where T : IEquatable<T>
            => IntersectToArray<T, StructEquatableComparer<T>>(first, second);
    }
}
