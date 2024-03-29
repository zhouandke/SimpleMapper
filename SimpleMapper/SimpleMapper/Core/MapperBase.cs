﻿using System;

namespace ZK.Mapper.Core
{
    /// <summary>
    /// Mapper基础类, 派生类都必须是 《TSource, Target》的强类型
    /// </summary>
    public abstract class MapperBase
    {
        protected MapperBase(TypePair typePair, IRootMapper rootMapper)
        {
            TypePair = typePair;
            RootMapper = rootMapper;
        }

        public TypePair TypePair { get; }

        public IRootMapper RootMapper { get; }

        public object Map(object source, object target, MapContext mapContext)
        {
            target = MapCore(source, target, mapContext);

            Func<object, object, object> postFunc;
            if (RootMapper.PostActionDicts.TryGetValue(TypePair, out postFunc))
            {
                target = postFunc(source, target);
            }
            return target;
        }

        protected abstract object MapCore(object source, object target, MapContext mapContext);
    }
}
