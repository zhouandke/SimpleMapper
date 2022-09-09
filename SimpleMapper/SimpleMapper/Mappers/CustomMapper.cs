using System;
using ZK.Mapper.Core;

namespace ZK.Mapper.Mappers
{
    /// <summary>
    /// 用户自定义的映射
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TTarget"></typeparam>
    public class CustomMapper<TSource, TTarget> : MapperBase
    {
        private readonly Func<TSource, TTarget, TTarget> customFunc;

        public CustomMapper(Func<TSource, TTarget, TTarget> customFunc, IRootMapper rootMapper)
            : base(new TypePair(typeof(TSource), typeof(TTarget)), rootMapper)
        {
            this.customFunc = customFunc;
        }

        protected override object MapCore(object source, object target)
        {
            var src = source is TSource ? (TSource)source : default;
            var dst = target is TTarget ? (TTarget)target : default;
            var newTarget = customFunc(src, dst);
            return newTarget;
        }
    }
}
