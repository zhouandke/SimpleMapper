using System;
using System.Collections.Concurrent;

namespace ZK.Mapper.Core
{
    public interface IRootMapper
    {
        ConcurrentDictionary<TypePair, MapperBase> MapperBaseDicts { get; }

        ConcurrentDictionary<TypePair, Action<object, object>> PostActionDicts { get; }

        object Map(Type sourceType, Type targetType, object source, object target = null);
    }
}
