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
            UIntPtr i = (UIntPtr)1;
            var c = (Colors)i;
            //var a = (TypeCode?)TypeCode.Boolean;
            var a = 3.5f;
            //var k = (decimal)a;
            var obj =  1L;
            var func = BuildConvert<float, TypeCode>();
            
            var r=func(a);

            //var from = new
            //{
            //    A = Colors.Green,
            //    B = Colors.Red,
            //    C = Colors.Yellow,
            //    D = 18

            //};


            //var k = (Colors)TypeCode.Decimal;


            //var a = ZK.Mapper.SimpleMapper.Default.Map<MyClass>(from);
        }

        public static Func<TSource, TTarget> BuildConvert<TSource, TTarget>()
        {
            var sourceType = typeof(TSource);
            var targetType = typeof(TTarget);
            var sourceParam = Expression.Parameter(sourceType, "o");
            var convertExpr = Expression.Convert(sourceParam, targetType);
            return Expression.Lambda<Func<TSource, TTarget>>(convertExpr, new[] { sourceParam }).Compile();

        }

        static void Act(ref Point point)
        {
            point.X = 99;
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

    public class A
    {
        public int Id { get; set; }

        public int Age { get; set; }

        public int? Height { get; set; }

        public float Weight { get; set; }

        public decimal? Score { get; set; }

        public bool Death { get; set; }

        public Point? Point1 { get; set; }

        public Point Point2 { get; set; }
    }


    public class ADto
    {
        public int Id { get; set; }

        public int? Age { get; set; }

        public float Height { get; set; }

        //public double? Weight { get; set; }

        //public float Score { get; set; }

        //public decimal Death { get; set; }
        //public Point Point1 { get; set; }
        //public Point? Point2 { get; set; }
    }

    public struct Point
    {
        public int X { get; set; }
    }
}
