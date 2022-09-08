using System;
using ZK.Mapper.Core;
using ZK.Mapper.Help;

namespace ZK.Mapper.Mappers
{
    public class PrimitiveTypeMapper<TSource, TTarget, TSourcePrimitve, TTargetPrimitve> : MapperBase
    {
        bool isSourceNullable;
        bool isTargetNullable;
        Type targetType;
        Type targetPrimitveType;

        public PrimitiveTypeMapper(IRootMapper rootMapper)
            : base(new TypePair(typeof(TSource), typeof(TTarget)), rootMapper)
        {
            isSourceNullable = TypeHelp.IsNullable(typeof(TSource));
            isTargetNullable = TypeHelp.IsNullable(typeof(TTarget));
            targetType = typeof(TTarget);
            targetPrimitveType = typeof(TTargetPrimitve);
        }

        protected override object MapCore(object source, object target)
        {
            if (TypePair.SourceType == TypePair.TargetType)
            {
                return source;
            }
            if (source == null)
            {
                return target ?? default(TTarget);
            }

            if (isTargetNullable)
            {
                // return (TTarget)source; // will throw  Unable to cast object of type 'System.Single' to type 'System.Double'.  why ?
                return Convert.ChangeType(source, targetPrimitveType);
            }
            else
            {
                return Convert.ChangeType(source, targetType); // Convert.ChangeType not support Nullable<>
            }
        }
    }

    public class PrimitiveTypeMapperBuilder : MapperBuilderBase
    {
        public PrimitiveTypeMapperBuilder(IRootMapper rootMapper) : base(rootMapper)
        {
        }

        public override int Priority => 1000;

        public override MapperBase Build(TypePair typePair)
        {
            if (TypeHelp.IsPrimitiveNullable(typePair.SourceType, out var sourcePrimitveType)
                && TypeHelp.IsPrimitiveNullable(typePair.TargetType, out var targetPrimitveType))
            {
                var type = typeof(PrimitiveTypeMapper<,,,>).MakeGenericType(typePair.SourceType, typePair.TargetType, sourcePrimitveType, targetPrimitveType);
                var mapper = (MapperBase)type.GetConstructor(ImplementMapperConstructorTypes).Invoke(new object[] { RootMapper });
                return mapper;
            }

            return null;
        }
    }
}
