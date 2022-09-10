using System;
using ZK.Mapper.Core;
using ZK.Mapper.Help;

namespace ZK.Mapper.Mappers
{
    /// <summary>
    /// Nullable<T> 与 T 的转换
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TTarget"></typeparam>
    internal class NullableMapper<TSource, TTarget> : MapperBase
    {
        public NullableMapper(IRootMapper rootMapper)
            : base(new TypePair(typeof(TSource), typeof(TTarget)), rootMapper)
        { }

        protected override object MapCore(object source, object target, MapContext mapContext)
        {
            if (TypePair.SourceType == TypePair.TargetType)
            {
                return source;
            }
            if (source == null)
            {
                return target ?? default(TTarget);
            }

            return source;
        }
    }


    internal class NullableMapperBuilder : MapperBuilderBase
    {
        public NullableMapperBuilder(IRootMapper rootMapper) : base(rootMapper)
        {
        }

        public override int Priority => 900;

        public override MapperBase Build(TypePair typePair)
        {
            if (TypeHelp.IsNullable(typePair.SourceType, out var sourcUnderlyingType) | TypeHelp.IsNullable(typePair.TargetType, out var targetUnderlyingType)
                && sourcUnderlyingType == targetUnderlyingType)
            {
                var type = typeof(NullableMapper<,>).MakeGenericType(typePair.SourceType, typePair.TargetType);
                var mapper = (MapperBase)type.GetConstructor(ImplementMapperConstructorTypes).Invoke(new object[] { RootMapper });
                return mapper;
            }

            return null;
        }
    }
}
