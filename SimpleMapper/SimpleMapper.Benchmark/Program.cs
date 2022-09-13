using BenchmarkDotNet.Running;
using System;

namespace SimpleMapper.Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            var c = new CollectionBenchmark();
            for (int i = 0; i < 1000000; i++)
            {
                c.CollectionMapping_SimpleMapperAsDeepCopy();
            }
            return;
            //BenchmarkRunner.Run<PrimitiveTypeBenchmark>();
            BenchmarkRunner.Run<CollectionBenchmark>();
            Console.WriteLine("Hello World!");
        }
    }
}
