using System;

namespace ZK
{
    public class PrimitiveTypeMapper<TSource, TTarget, TSourcePrimitve, TTargetPrimitve> : MapperBase
    {
        bool isSourceNullable;
        bool isTargetNullable;
        Type targetType;

        public PrimitiveTypeMapper(IRootMapper rootMapper)
            : base(new TypePair(typeof(TSource), typeof(TTarget)), rootMapper)
        {
            isSourceNullable = TypeHelp.IsNullable(typeof(TSource));
            isTargetNullable = TypeHelp.IsNullable(typeof(TTarget));
            targetType = typeof(TTarget);
        }

        protected override object MapCore(object source, object target)
        {
            if (TypePair.SourceType == TypePair.TargetType)
            {
                return source;
            }

            if (source == null)
            {
                return default(TTarget);
            }
            if (isTargetNullable)
            {
                return (TTarget)source;
            }

            return Convert.ChangeType(source, targetType);
        }
    }
}
