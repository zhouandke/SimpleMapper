using System;
using System.Collections.Generic;

namespace ConsoleApp5
{
    class Program
    {
        static void Main(string[] args)
        {
            var a = new A
            {
                Id = 100,
                Age = 21,
                B = new B
                {
                    Name = "KK",
                    Address = 999
                },
                CList = new List<C>{ new C { Color1 = Color.Red, Color2 = 2 } }
            };
            var aDto = new ADto();

            HigLabo.Core.ObjectMapper.Default.Map(a, aDto);

            Console.WriteLine("Hello World!");
        }
    }


    public class A
    {
        public int? Id { get; set; }

        public int Age { get; set; }

        public B B { get; set; }

        public List<C> CList { get; set; }

    }

    public class ADto
    {
        public int Id { get; set; }

        public string Age { get; set; }

        public BDto B { get; set; }

        public List<CDto> CList { get; set; }
    }


    public class B
    {
        public string Name { get; set; }
        public int Address { get; set; }
    }

    public class BDto
    {
        public string Name { get; set; }

        public string Address { get; set; }

    }


    public class C
    {
        public Color Color1 { get; set; }

        public int Color2 { get; set; }
    }

    public class CDto
    {
        public string Color1 { get; set; }

        public Color Color2 { get; set; }
    }


    public enum Color
    {
        Black = 0,
        Red = 1
    }
}
