using System;

namespace ZK
{
    public class NullableMapper<TSource, TTarget> : MapperBase
    {

        public NullableMapper(IRootMapper rootMapper)
            : base(new TypePair(typeof(TSource), typeof(TTarget)), rootMapper)
        {
        }

        protected override object MapCore(object source, object target)
        {
            throw new Exception();
        }
    }
}
