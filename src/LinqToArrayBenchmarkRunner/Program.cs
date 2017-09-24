using BenchmarkDotNet.Running;
using LinqToArray;
using LinqToArrayUnitTest;

namespace LinqToArrayBenchmarkRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            //BenchmarkRunner.Run<LinqToArrayUnitTest.Benchmarks.GroupByIntBenchamark>();
            BenchmarkRunner.Run<LinqToArrayUnitTest.Benchmarks.GroupByStringBenchamark>();
            //BenchmarkRunner.Run<LinqToArrayUnitTest.Benchmarks.ToDictionaryIntBenchamark>();
            //BenchmarkRunner.Run<LinqToArrayUnitTest.Benchmarks.SelectIntBenchamark>();
        }
    }
}
