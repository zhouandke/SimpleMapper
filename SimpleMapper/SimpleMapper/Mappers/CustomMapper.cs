using System;
using ZK.Mapper.Core;

namespace ZK.Mapper.Mappers
{
    public class CustomMapper<TSource, TTarget> : MapperBase
    {
        private Func<TSource, TTarget, TTarget> customFunc;

        public CustomMapper(Func<TSource, TTarget, TTarget> customFunc, IRootMapper rootMapper)
            : base(new TypePair(typeof(TSource), typeof(TTarget)), rootMapper)
        {
            this.customFunc = customFunc;
        }

        protected override object MapCore(object source, object target)
        {
            var newTarget = customFunc((TSource)source, (TTarget)target);
            return newTarget;
        }
    }
}
