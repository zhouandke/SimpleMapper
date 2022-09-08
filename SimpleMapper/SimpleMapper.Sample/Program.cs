using System;
using System.Linq.Expressions;
using System.Reflection;
using ZK;

namespace SimpleMapper.Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            Expression<Func<float, double>> expr = o => (double)o;

            var f = expr.Compile();
            f(1f);

            var a = new A
            {
                Id = 1,
                Age = 33,
                Height = null,
                Weight = 62,
                Death = true
            };




            var b = ZK.SimpleMapper.Default.Map<ADto>(a);
        }

    }

    public class A
    {
        public int Id { get; set; }

        public int Age { get; set; }

        public int? Height { get; set; }

        public float Weight { get; set; }

        public bool Death { get; set; }

    }


    public class ADto
    {
        public int Id { get; set; }

        public int? Age { get; set; }

        public int Height { get; set; }

        public double? Weight { get; set; }

        public decimal Death { get; set; }
    }
}
