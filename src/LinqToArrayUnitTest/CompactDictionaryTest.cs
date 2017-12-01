using LinqToArray;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace LinqToArrayUnitTest
{
    public class CompactDictionaryTest
    {
        [Fact]
        public void TryGet()
        {
            var d = new CompactDictionary<string, int>(items);

            foreach (var n in notExsist) Assert.Null(Get(d, n));
            foreach (var n in items) Assert.Equal(n.Value, Get(d, n.Key));
        }

        static int? Get(CompactDictionary<string, int> x, string s) => x.TryGetValue(s, out var v) ? v : default(int?);

        static KeyValuePair<string, int> Kvp(string s, int i) => new KeyValuePair<string, int>(s, i);
        static KeyValuePair<string, int>[] items = new[]
        {
            Kvp("zero", 0),
            Kvp("one", 1),
            Kvp("two", 2),
            Kvp("three", 3),
            Kvp("four", 4),
            Kvp("five", 5),
            Kvp("six", 6),
            Kvp("seven", 7),
            Kvp("eight", 8),
            Kvp("nine", 9),
        };
        static string[] notExsist = new[] { "a", "b", "abc", "one1", "on", "o", "one12", "" };

        [Fact]
        public void MarginalCapacity()
        {
            var d = new CompactDictionary<string, int>(items.Length / 2);
            foreach (var n in items) d.AddOrUpdate(n.Key, n.Value);

            foreach (var n in notExsist) Assert.Null(Get(d, n));
            foreach (var n in items) Assert.Equal(n.Value, Get(d, n.Key));
        }
    }
}
