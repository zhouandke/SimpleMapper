using System;

namespace ZK
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
}
