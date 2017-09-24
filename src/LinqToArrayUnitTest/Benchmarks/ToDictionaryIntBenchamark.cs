using System.Linq;
using BenchmarkDotNet.Attributes;
using LinqToArray;
using System.Collections.Generic;

namespace LinqToArrayUnitTest.Benchmarks
{
    [MemoryDiagnoser]
    public class ToDictionaryIntBenchamark
    {
        [Params(0, 1, 2, 3, 4, 5)]
        public int Index { get; set; }

        [Benchmark(Baseline = true)]
        public IReadOnlyDictionary<int, int> SystemLinq() => TestData.IntData[Index].Distinct().ToDictionary(x => x * x);

        [Benchmark]
        public IReadOnlyDictionary<int, int> ArrayOpt() => TestData.IntData[Index].DistinctToArray().ToComapctDictionary(x => x * x);
    }
}
