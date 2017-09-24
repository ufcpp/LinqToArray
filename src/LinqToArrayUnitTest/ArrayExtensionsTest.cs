using System.Linq;
using LinqToArray;
using Xunit;
using System.Collections.Generic;

namespace LinqToArrayUnitTest
{
    public class ArrayExtensionsTest
    {
        [Theory, ClassData(typeof(TestSequence1))]
        public void Select(object list)
        {
            if (list is int[] i)
            {
                var expected = i.Select(x => x * x).ToArray();
                var actual = i.SelectToArray(x => x * x);
                Assert.Equal(expected, actual);
            }
            else if (list is string[] s)
            {
                var expected = s.Select(x => x.Length).ToArray();
                var actual = s.SelectToArray(x => x.Length);
                Assert.Equal(expected, actual);
            }
        }

        [Theory, ClassData(typeof(TestSequence1))]
        public void Where(object list)
        {
            if (list is int[] i)
            {
                var expected = i.Where(x => x % 2 == 1).ToArray();
                var actual = i.WhereToArray(x => x % 2 == 1);
                Assert.Equal(expected, actual);
            }
            else if (list is string[] s)
            {
                var expected = s.Where(x => x.Length < 4).ToArray();
                var actual = s.WhereToArray(x => x.Length < 4);
                Assert.Equal(expected, actual);
            }
        }

        [Theory, ClassData(typeof(TestSequence1))]
        public void WhereSelect(object list)
        {
            if (list is int[] i)
            {
                var expected = i.Where(x => x % 2 == 1).Select(x => x * x).ToArray();
                var actual = i.WhereSelectToArray(x => x % 2 == 1, x => x * x);
                Assert.Equal(expected, actual);
            }
            else if (list is string[] s)
            {
                var expected = s.Where(x => x.Length < 4).Select(x => x[0]).ToArray();
                var actual = s.WhereSelectToArray(x => x.Length < 4, x => x[0]);
                Assert.Equal(expected, actual);
            }
        }

        [Theory, ClassData(typeof(TestSequence1))]
        public void Distinct(object list)
        {
            if (list is int[] i)
            {
                var expected = i.Distinct().ToArray();
                var actual = i.DistinctToArray();
                Assert.Equal(expected, actual);
            }
            else if (list is string[] s)
            {
                var expected = s.Distinct().ToArray();
                var actual = s.DistinctToArray();
                Assert.Equal(expected, actual);
            }
        }

        [Theory, ClassData(typeof(TestSequence1))]
        public void ToDictionary(object list)
        {
            if (list is int[] i)
            {
                var expected = i.Distinct().ToDictionary(x => x);
                var actual = i.DistinctToArray().ToComapctDictionary(x => x);
                Assert.Equal(expected, actual, DictionaryComparer<int, int>.Default);
            }
            else if (list is string[] s)
            {
                var expected = s.Distinct().ToDictionary(x => x);
                var actual = s.DistinctToArray().ToComapctDictionary(x => x);
                Assert.Equal(expected, actual, DictionaryComparer<string, string>.Default);
            }
        }

        private static Dictionary<TKey, TValue[]> D<TKey, TValue>(IEnumerable<IGrouping<TKey, TValue>> items)
        {
            var d = new Dictionary<TKey, TValue[]>();
            foreach (var item in items)
            {
                d.Add(item.Key, item.ToArray());
            }
            return d;
        }

        private static Dictionary<TKey, TValue[]> D<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue[]>> items)
        {
            var d = new Dictionary<TKey, TValue[]>();
            foreach (var item in items)
            {
                d.Add(item.Key, item.Value);
            }
            return d;
        }

        [Theory, ClassData(typeof(TestSequence1))]
        public void GroupBy(object list)
        {
            if (list is int[] i)
            {
                var expected = Enumerable.GroupBy(i, x => x, x => x * x);
                var actual = i.GroupByToArray(x => x, x => x * x);
                Assert.Equal(D(expected), D(actual), new DictionaryComparer<int, int[]>(ArrayComparer<int>.Default));
            }
            else if (list is string[] s)
            {
                var expected = Enumerable.GroupBy(s, x => x.Length);
                var actual = s.GroupByToArray(x => x.Length);
                Assert.Equal(D(expected), D(actual), new DictionaryComparer<int, string[]>(ArrayComparer<string>.Default));
            }
        }

        //todo: Count, Any, All
        //todo: Except, Intersect
    }
}
