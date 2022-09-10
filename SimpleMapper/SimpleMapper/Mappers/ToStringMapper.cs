using System;
using ZK.Mapper.Core;
using ZK.Mapper.Help;

namespace ZK.Mapper.Mappers
{
    /// <summary>
    /// object 转换为 string
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    internal class ToStringMapper<TSource> : MapperBase
    {
        public ToStringMapper(IRootMapper rootMapper)
            : base(new TypePair(typeof(TSource), typeof(string)), rootMapper)
        { }

        protected override object MapCore(object source, object target, MapContext mapContext)
        {
            return source?.ToString();
        }
    }


    internal class ToStringMapperBuilder : MapperBuilderBase
    {
        public ToStringMapperBuilder(IRootMapper rootMapper) : base(rootMapper)
        {
        }

        public override int Priority => 100;

        public override MapperBase Build(TypePair typePair)
        {
            if (typePair.TargetType == typeof(string))
            {
                var type = typeof(ToStringMapper<>).MakeGenericType(typePair.SourceType);
                var mapper = (MapperBase)type.GetConstructor(ImplementMapperConstructorTypes).Invoke(new object[] { RootMapper });
                return mapper;
            }

            return null;
        }
    }
}
