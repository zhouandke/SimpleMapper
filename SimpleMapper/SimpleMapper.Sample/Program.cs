using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using ZK.Mapper.Help;

namespace SimpleMapper.Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            var a = new A
            {
                MyProperty = new MyClass()
            };

            var b = a;
            var u = object.ReferenceEquals(b.MyProperty, a.MyProperty);

            var k = TypeHelp.IsImmutable(typeof(KeyValuePair<int, int>));
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

     struct A
    {
        public MyClass MyProperty { get; set; }
    }

}
