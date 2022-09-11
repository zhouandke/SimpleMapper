using BenchmarkDotNet.Running;
using System;

namespace SimpleMapper.Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            //var c = new CollectionBenchmark();
            //c.CollectionMapping_SimpleMapperAsDeepCopy();
            //BenchmarkRunner.Run<PrimitiveTypeBenchmark>();
            BenchmarkRunner.Run<CollectionBenchmark>();
            Console.WriteLine("Hello World!");
        }
    }
}
