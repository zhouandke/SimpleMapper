using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using ZK.Mapper.Help;

namespace ZK.Mapper.Core
{
    public class ImmutableTypeManage
    {
        private ConcurrentDictionary<Type, bool> ImmutableTypeDict { get; } = new ConcurrentDictionary<Type, bool>();

        public void RegisterImmutableType(Type type, bool isImmutable)
        {
            ImmutableTypeDict[type] = isImmutable;
        }

        public bool IsImmutable(Type type)
        {
            if (type.IsGenericType)
            {
                type = type.GetGenericTypeDefinition();
            }

            return ImmutableTypeDict.GetOrAdd(type, t => TypeHelp.IsImmutable(t)); ;
        }
    }
}
