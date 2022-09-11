using System;
using AutoMapper;
using BenchmarkDotNet.Attributes;
using Nelibur.ObjectMapper;

namespace SimpleMapper.Benchmark
{
    public class PrimitiveTypeBenchmark
    {
        private readonly SourceWithPrimitiveTypes _source = CreateSource();
        private IMapper autoMapper;

        public PrimitiveTypeBenchmark()
        {
            InitTinyMapper();
            InitTinyAutoMapper();
        }

        private void InitTinyMapper()
        {
            TinyMapper.Bind<SourceWithPrimitiveTypes, TargetWithPrimitiveTypes>();
        }

        private void InitTinyAutoMapper()
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap<SourceWithPrimitiveTypes, TargetWithPrimitiveTypes>());
            autoMapper = config.CreateMapper();
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

        [Benchmark]
        public void BenchmarkTinyMapper()
        {
            TinyMapper.Map<SourceWithPrimitiveTypes, TargetWithPrimitiveTypes>(_source);
        }

        [Benchmark]
        public void BenchmarkAutoMapper()
        {
            autoMapper.Map<TargetWithPrimitiveTypes>(_source);
        }

        [Benchmark]
        public void BenchmarkSimpleMapper()
        {
            ZK.Mapper.SimpleMapper.Default.Map<TargetWithPrimitiveTypes>(_source);
        }

        [Benchmark]
        public void BenchmarkHandwritten()
        {
            var target = new TargetWithPrimitiveTypes();
            target.Bool = _source.Bool;
            target.Byte = _source.Byte;
            target.Char = _source.Char;
            target.DateTime = _source.DateTime;
            target.Decimal = _source.Decimal;
            target.Float = _source.Float;
            target.Int = _source.Int;
            target.Long = _source.Long;
            target.Short = _source.Short;
            target.FirstName = _source.FirstName;
            target.LastName = _source.LastName;
            target.Nickname = _source.Nickname;
            target.Email = _source.Email;
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
