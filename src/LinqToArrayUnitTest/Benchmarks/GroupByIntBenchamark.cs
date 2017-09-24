using System.Linq;
using BenchmarkDotNet.Attributes;
using LinqToArray;
using System.Collections.Generic;

namespace LinqToArrayUnitTest.Benchmarks
{
    [MemoryDiagnoser]
    public class GroupByIntBenchamark
    {
        [Params(0, 1, 2, 3, 4, 5)]
        public int Index { get; set; }

        [Benchmark(Baseline = true)]
        public IEnumerable<IGrouping<int, int>> SystemLinq() => Enumerable.GroupBy(TestData.IntData[Index], x => x % 3).ToArray();

        [Benchmark]
        public IEnumerable<KeyValuePair<int, int[]>> ArrayOpt() => ArrayExtensions.GroupByToArray(TestData.IntData[Index], x => x % 3);
    }
}
