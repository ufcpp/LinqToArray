using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace LinqToArray
{
    internal unsafe struct StackHashSet<T, TComp>
        where TComp : struct, IEqualityComparer<T>
    {
        internal const int Skip = 928191829;

        private T[] items;
        private int* buckets;
        int len;
        int mask;

        public StackHashSet(T[] items, int* buckets, int len)
        {
            this.items = items;
            this.buckets = buckets;
            this.len = len;
            mask = len - 1;

            for (int i = 0; i < len; i++) buckets[i] = -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(T item)
        {
            var hash = default(TComp).GetHashCode(item) & mask;
            while (true)
            {
                ref var b = ref buckets[hash];

                if (b == -1)
                {
                    return false;
                }
                else if (default(TComp).Equals(item, items[b]))
                {
                    return true;
                }

                hash = (hash + Skip) & mask;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MakeHashtTable()
        {
            var mask = len - 1;
            for (int i = 0; i < items.Length; i++)
            {
                Add(items[i], i);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Add(T item, int i)
        {
            var hash = default(TComp).GetHashCode(item) & mask;

            while (true)
            {
                ref var b = ref buckets[hash];

                if (b == -1)
                {
                    b = i;
                    return false;
                }
                else if (default(TComp).Equals(item, items[b]))
                {
                    return true;
                }

                hash = (hash + Skip) & mask;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetOrAdd(T item, int i)
        {
            var hash = default(TComp).GetHashCode(item) & mask;

            while (true)
            {
                ref var b = ref buckets[hash];

                if (b == -1)
                {
                    b = i;
                    return b;
                }
                else if (default(TComp).Equals(item, items[b]))
                {
                    return b;
                }

                hash = (hash + Skip) & mask;
            }
        }
    }
}
