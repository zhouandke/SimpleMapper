using System;
using System.Linq;
using System.Linq.Expressions;
using ZK.Mapper.Core;
using ZK.Mapper.Help;

namespace ZK.Mapper.Mappers
{
    internal class EnumMapper<TSource, TTarget, TSourceUnderlyingType, TTargetUnderlyingType> : MapperBase
        where TTargetUnderlyingType : struct
    {
        private static readonly Type sourceType = typeof(TSource);
        private static readonly Type targetType = typeof(TTarget);
        private static readonly Type sourceUnderlyingType = typeof(TSourceUnderlyingType);
        private static readonly Type targetUnderlyingType = typeof(TTargetUnderlyingType);

        private EnumMapperType enumMapperType;
        private Func<TSource, object> convert;
        public EnumMapper(IRootMapper rootMapper, EnumMapperType enumMapperType)
            : base(new TypePair(typeof(TSource), typeof(TTarget)), rootMapper)
        {
            this.enumMapperType = enumMapperType;
            switch (enumMapperType)
            {
                case EnumMapperType.EnumToNumber:
                    convert = EnumToNumber();
                    break;
                case EnumMapperType.EnumToEnum:
                    convert = EnumToEnum();
                    break;
                case EnumMapperType.ToEnum:
                    convert = ToEnum;
                    break;
                case EnumMapperType.NotSupport:
                default:
                    convert = src => default(TTarget);
                    break;
            }
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

            return convert((TSource)source);
        }

        private Func<TSource, object> EnumToNumber()
        {
            var sourceParam = Expression.Parameter(sourceType, "o");
            var convertExpr = Expression.Convert(sourceParam, sourceUnderlyingType);
            convertExpr = Expression.Convert(convertExpr, targetType);
            convertExpr = Expression.Convert(convertExpr, typeof(object));
            return Expression.Lambda<Func<TSource, object>>(convertExpr, new[] { sourceParam }).Compile();
        }

        private Func<TSource, object> EnumToEnum()
        {
            var sourceParam = Expression.Parameter(sourceType, "o");
            var convertExpr = Expression.Convert(sourceParam, targetType);
            convertExpr = Expression.Convert(convertExpr, typeof(object));
            return Expression.Lambda<Func<TSource, object>>(convertExpr, new[] { sourceParam }).Compile();
        }

        private object ToEnum(TSource source)
        {
            if (Enum.TryParse(source.ToString(), true, out TTargetUnderlyingType result))
            {
                return result;
            }

            return default(TTarget);
        }
    }


    internal class EnumMapperBuilder : MapperBuilderBase
    {
        public EnumMapperBuilder(IRootMapper rootMapper) : base(rootMapper)
        {
        }

        public override int Priority => 1100;

        public override MapperBase Build(TypePair typePair)
        {
            EnumMapperType enumMapperType;
            if (TypeHelp.IsEnum(typePair.SourceType, out var sourcUnderlyingType) && TypeHelp.IsNumberForEnumMap(typePair.TargetType, out var targetNumberType))
            {
                enumMapperType = EnumMapperType.EnumToNumber;
                var type = typeof(EnumMapper<,,,>).MakeGenericType(typePair.SourceType, typePair.TargetType, sourcUnderlyingType, targetNumberType);
                var mapper = (MapperBase)type.GetConstructor(new Type[] { typeof(IRootMapper), typeof(EnumMapperType) }).Invoke(new object[] { RootMapper, enumMapperType });
                return mapper;
            }
            if (TypeHelp.IsEnum(typePair.SourceType, out sourcUnderlyingType) && TypeHelp.IsEnum(typePair.TargetType, out var targetUnderlyingType))
            {
                enumMapperType = EnumMapperType.EnumToEnum;
                var type = typeof(EnumMapper<,,,>).MakeGenericType(typePair.SourceType, typePair.TargetType, sourcUnderlyingType, targetUnderlyingType);
                var mapper = (MapperBase)type.GetConstructor(new Type[] { typeof(IRootMapper), typeof(EnumMapperType) }).Invoke(new object[] { RootMapper, enumMapperType });
                return mapper;
            }
            if (TypeHelp.IsNumberForEnumMap(typePair.SourceType, out var sourceNumberType) && TypeHelp.IsEnum(typePair.TargetType, out targetUnderlyingType))
            {
                enumMapperType = EnumMapperType.ToEnum;
                var type = typeof(EnumMapper<,,,>).MakeGenericType(typePair.SourceType, typePair.TargetType, sourceNumberType, targetUnderlyingType);
                var mapper = (MapperBase)type.GetConstructor(new Type[] { typeof(IRootMapper), typeof(EnumMapperType) }).Invoke(new object[] { RootMapper, enumMapperType });
                return mapper;
            }
            if (typePair.SourceType == typeof(string) && TypeHelp.IsEnum(typePair.TargetType, out targetUnderlyingType))
            {
                enumMapperType = EnumMapperType.ToEnum;
                var type = typeof(EnumMapper<,,,>).MakeGenericType(typePair.SourceType, typePair.TargetType, typeof(string), targetUnderlyingType);
                var mapper = (MapperBase)type.GetConstructor(new Type[] { typeof(IRootMapper), typeof(EnumMapperType) }).Invoke(new object[] { RootMapper, enumMapperType });
                return mapper;
            }

            return null;
        }
    }

    internal enum EnumMapperType
    {
        NotSupport = 0,
        EnumToNumber = 1,
        EnumToEnum = 2,
        ToEnum = 3
    }
}
