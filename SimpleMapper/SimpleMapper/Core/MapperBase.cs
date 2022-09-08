using System;

namespace ZK.Mapper.Core
{
    public abstract class MapperBase
    {
        protected MapperBase(TypePair typePair, IRootMapper rootMapper)
        {
            TypePair = typePair;
            RootMapper = rootMapper;
        }

        public TypePair TypePair { get; }

        public IRootMapper RootMapper { get; }

        public object Map(object source, object target)
        {
            target = MapCore(source, target);

            Action<object, object> postAction;
            if (RootMapper.PostActionDicts.TryGetValue(TypePair, out postAction))
            {
                postAction(source, target);
            }
            return target;
        }

        protected abstract object MapCore(object source, object target);
    }
}
