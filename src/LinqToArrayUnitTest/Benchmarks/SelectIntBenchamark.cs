using System.Linq;
using BenchmarkDotNet.Attributes;
using LinqToArray;

namespace LinqToArrayUnitTest.Benchmarks
{
    [MemoryDiagnoser]
    public class SelectIntBenchamark
    {
        [Params(0, 1, 2, 3, 4, 5)]
        public int Index { get; set; }

        [Benchmark(Baseline = true)]
        public int[] SystemLinq() => TestData.IntData[Index].Select(x => x * x).ToArray();

        [Benchmark]
        public int[] ArrayOpt() => TestData.IntData[Index].SelectToArray(x => x * x);
    }
}
