using System;
using System.Linq.Expressions;
using System.Reflection;

namespace SimpleMapper.Sample
{
    class Program
    {
        static void Main(string[] args)
        {



            var a = new A
            {
                Id = 1,
                Age = 33,
                Height = null,
                Weight = 62,
                Death = true,
                Point1 = null, // new Point() { X = 1 },
                Point2 = new Point() { X = 2 }
            };




            var b = ZK.Mapper.SimpleMapper.Default.Map<ADto>(a);
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

        public double? Weight { get; set; }

        public float Score { get; set; }

        public decimal Death { get; set; }
        public Point Point1 { get; set; }
        public Point? Point2 { get; set; }
    }

    public struct Point
    {
        public int X { get; set; }
    }
}
