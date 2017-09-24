using System.Linq;
using BenchmarkDotNet.Attributes;
using LinqToArray;
using System.Collections.Generic;

namespace LinqToArrayUnitTest.Benchmarks
{
    [MemoryDiagnoser]
    public class GroupByStringBenchamark
    {
        [Params(0, 1, 2)]
        public int Index { get; set; }

        [Benchmark(Baseline = true)]
        public IEnumerable<IGrouping<int, string>> SystemLinq() => TestData.StrData[Index].GroupBy(x => x.Length).ToArray();

        [Benchmark]
        public IEnumerable<KeyValuePair<int, string[]>> ArrayOpt() => TestData.StrData[Index].GroupByToArray(x => x.Length);
    }
}
