using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZK
{
    class Program
    {
        static void Main(string[] args)
        {
            var source = CreateSource();
            var target = ZK.SimpleMapper.Map<TargetWithPrimitiveTypes>(source);
            var s1 = Newtonsoft.Json.JsonConvert.SerializeObject(source);
            var s2 = Newtonsoft.Json.JsonConvert.SerializeObject(target);

            if (s1 == s2)
            {

            }
        }

        private static SourceWithPrimitiveTypes CreateSource()
        {
            return new SourceWithPrimitiveTypes
            {
                FirstName = "John",
                LastName = "Doe",
                Nickname = "TinyMapper",
                Email = "support@TinyMapper.net",
                Short = 3,
                Long = 10,
                Int = 5,
                Float = 4.9f,
                Decimal = 4.0m,
                DateTime = DateTime.Now,
                Char = 'a',
                Bool = true,
                Byte = 0
            };
        }

    }

    public sealed class SourceWithPrimitiveTypes
    {
        public bool Bool { get; set; }
        public byte Byte { get; set; }
        public char Char { get; set; }
        public DateTime DateTime { get; set; }
        public decimal Decimal { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public float Float { get; set; }
        public int Int { get; set; }
        public string LastName { get; set; }
        public long Long { get; set; }
        public string Nickname { get; set; }
        public short Short { get; set; }
    }


    public sealed class TargetWithPrimitiveTypes
    {
        public bool Bool { get; set; }
        public byte Byte { get; set; }
        public char Char { get; set; }
        public DateTime DateTime { get; set; }
        public decimal Decimal { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public float Float { get; set; }
        public int Int { get; set; }
        public string LastName { get; set; }
        public long Long { get; set; }
        public string Nickname { get; set; }
        public short Short { get; set; }
    }
}
