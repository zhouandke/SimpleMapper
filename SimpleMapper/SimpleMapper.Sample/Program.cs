using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace SimpleMapper.Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            var from = new
            {
                A = Colors.Green,
                B = Colors.Red,
                C = Colors.Yellow,
                D = 18
            };

            var to = ZK.Mapper.SimpleMapper.Default.Map<MyClass>(from);
        }
    }

    class MyClass
    {
        public int A { get; set; }

        public int? B { get; set; }

        public TypeCode C { get; set; }

        public Colors D { get; set; }
    }

    enum Colors
    {
        Unknown = 0,
        Red = 1,
        Yellow = 2,
        Green = 3
    }
}
