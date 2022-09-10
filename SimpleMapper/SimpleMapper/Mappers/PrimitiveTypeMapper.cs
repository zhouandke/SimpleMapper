using System;
using ZK.Mapper.Core;
using ZK.Mapper.Help;

namespace ZK.Mapper.Mappers
{
    /// <summary>
    /// 基本类型 + Decimal 的转换，包括 Nullable<>
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TTarget"></typeparam>
    /// <typeparam name="TSourcePrimitve"></typeparam>
    /// <typeparam name="TTargetPrimitve"></typeparam>
    internal class PrimitiveTypeMapper<TSource, TTarget, TSourcePrimitve, TTargetPrimitve> : MapperBase
    {
        private static readonly Type targetType = typeof(TTarget);
        private static readonly Type targetPrimitveType = typeof(TTargetPrimitve);
        private static readonly bool isTargetNullable = TypeHelp.IsNullable(typeof(TTarget));

        private Func<TSource, TTarget> convert;

        public PrimitiveTypeMapper(IRootMapper rootMapper)
            : base(new TypePair(typeof(TSource), typeof(TTarget)), rootMapper)
        {
            // bool 与 其他 number 类型转换确实很麻烦
            if (typeof(TSourcePrimitve) == typeof(bool) || typeof(TTargetPrimitve) == typeof(bool))
            {
                convert = src =>
                {
                    if (isTargetNullable)
                    {
                        return (TTarget)Convert.ChangeType(src, targetPrimitveType);
                    }
                    else
                    {
                        return (TTarget)Convert.ChangeType(src, targetType);
                    }
                };
            }
            else
            {
                convert = ExpressionGenerator.BuildConvert<TSource, TTarget>();
            }
        }

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

            return convert((TSource)source);

            //if (isTargetNullable)
            //{
            //    // return (TTarget)source; // will throw  Unable to cast object of type 'System.Single' to type 'System.Double'.  why ?
            //    return Convert.ChangeType(source, targetPrimitveType);
            //}
            //else
            //{
            //    return Convert.ChangeType(source, targetType); // Convert.ChangeType not support Nullable<>
            //}
        }
    }

    internal class PrimitiveTypeMapperBuilder : MapperBuilderBase
    {
        public PrimitiveTypeMapperBuilder(IRootMapper rootMapper) : base(rootMapper)
        {
        }

        public override int Priority => 1000;

        public override MapperBase Build(TypePair typePair)
        {
            if (TypeHelp.IsNumberForEnumMap(typePair.SourceType, out var sourcePrimitveType)
                && TypeHelp.IsNumberForEnumMap(typePair.TargetType, out var targetPrimitveType))
            {
                if (sourcePrimitveType.IsEnum && targetPrimitveType.IsEnum)
                {
                    // Convert.ChangeType 不能处理都是 enum 的情况
                    return null;
                }

                var type = typeof(PrimitiveTypeMapper<,,,>).MakeGenericType(typePair.SourceType, typePair.TargetType, sourcePrimitveType, targetPrimitveType);
                var mapper = (MapperBase)type.GetConstructor(ImplementMapperConstructorTypes).Invoke(new object[] { RootMapper });
                return mapper;
            }

            return null;
        }
    }
}
