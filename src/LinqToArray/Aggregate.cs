using System;

namespace LinqToArray
{
    public static partial class ArrayExtensions
    {
        /// <summary>
        /// Array optimized
        /// source.Count(predicate)
        /// </summary>
        public static int Count<TSource>(this TSource[] source, Func<TSource, bool> predicate)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            var count = 0;

            for (int i = 0; i < source.Length; i++)
            {
                if (predicate(source[i]))
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// Array optimized
        /// source.Any(predicate)
        /// </summary>
        public static bool Any<TSource>(this TSource[] source, Func<TSource, bool> predicate)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            for (int i = 0; i < source.Length; i++)
            {
                if (predicate(source[i])) return true;
            }

            return false;
        }

        /// <summary>
        /// Array optimized
        /// source.All(predicate)
        /// </summary>
        public static bool All<TSource>(this TSource[] source, Func<TSource, bool> predicate)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            for (int i = 0; i < source.Length; i++)
            {
                if (!predicate(source[i])) return false;
            }

            return true;
        }
    }
}
