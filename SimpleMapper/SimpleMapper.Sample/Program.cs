﻿using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace SimpleMapper.Sample
{
    class Program
    {
        static void Main(string[] args)
        {

            //var aType = typeof(A);
            //var emitFunc = Expression.Lambda<Func<object>>(Expression.New(aType)).Compile();

            //var count = 1000_000;
            //var sw = new Stopwatch();
            //sw.Restart();
            //for (int i = 0; i < count; i++)
            //{
            //    var a = new A();
            //}
            //sw.Stop();
            //Console.WriteLine(sw.ElapsedMilliseconds);

            //sw.Restart();
            //for (int i = 0; i < count; i++)
            //{
            //    var a = emitFunc();
            //}
            //sw.Stop();
            //Console.WriteLine(sw.ElapsedMilliseconds);

            //sw.Restart();
            //for (int i = 0; i < count; i++)
            //{
            //    var a = Activator.CreateInstance(aType);
            //}
            //sw.Stop();
            //Console.WriteLine(sw.ElapsedMilliseconds);


            var kkkk = typeof(Point).GetConstructor(Type.EmptyTypes);
            var a1 = new[] { "1", "2" };
            //var p = new Point() { X = 1 };
            //Act(ref p);


            //var a = new A
            //{
            //    Id = 1,
            //    Age = 33,
            //    Height = null,
            //    Weight = 62,
            //    Death = true,
            //    Point1 = null, // new Point() { X = 1 },
            //    Point2 = new Point() { X = 2 }
            //};

            //var b = ZK.Mapper.SimpleMapper.Default.Map<ADto>(a);
        }

        static void Act(ref Point point)
        {
            point.X = 99;
        }

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
