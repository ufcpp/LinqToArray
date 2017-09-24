using System;
using System.Collections.Generic;

namespace LinqToArray
{
    public static partial class ArrayExtensions
    {
        /// <summary>
        /// Array optimized
        /// source.Select(selector).ToArray()
        /// </summary>
        public static TResult[] SelectToArray<TSource, TResult>(this TSource[] source, Func<TSource, TResult> selector)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (selector == null) throw new ArgumentNullException(nameof(selector));

            var result = new TResult[source.Length];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = selector(source[i]);
            }

            return result;
        }

        /// <summary>
        /// Array optimized
        /// source.Where(predicate).ToArray()
        /// </summary>
        public static unsafe TSource[] WhereToArray<TSource>(this TSource[] source, Func<TSource, bool> predicate)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            var count = 0;
            var indexes = stackalloc int[source.Length];

            for (int i = 0; i < source.Length; i++)
            {
                if (predicate(source[i]))
                {
                    indexes[count] = i;
                    count++;
                }
                else
                {
                    indexes[i] = -1;
                }
            }


            var result = new TSource[count];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = source[indexes[i]];
            }

            return result;
        }

        /// <summary>
        /// Array optimized
        /// source.Where(predicate).Select(selector).ToArray()
        /// </summary>
        public static unsafe TResult[] WhereSelectToArray<TSource, TResult>(this TSource[] source, Func<TSource, bool> predicate, Func<TSource, TResult> selector)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (selector == null) throw new ArgumentNullException(nameof(selector));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            var count = 0;
            var indexes = stackalloc int[source.Length];

            for (int i = 0; i < source.Length; i++)
            {
                if (predicate(source[i]))
                {
                    indexes[count] = i;
                    count++;
                }
                else
                {
                    indexes[i] = -1;
                }
            }


            var result = new TResult[count];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = selector(source[indexes[i]]);
            }

            return result;
        }
    }
}
