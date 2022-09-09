using System;
using System.Collections.Concurrent;

namespace ZK.Mapper.Core
{
    /// <summary>
    /// Mapper 容器
    /// </summary>
    public interface IRootMapper
    {
        /// <summary>
        /// 缓存 TypePair 对应的 MapperBase
        /// </summary>
        ConcurrentDictionary<TypePair, MapperBase> MapperBaseDicts { get; }

        /// <summary>
        /// 保存 PostAction
        /// </summary>
        ConcurrentDictionary<TypePair, Action<object, object>> PostActionDicts { get; }

        /// <summary>
        /// 1. 使用 source 的属性新建一个 target(前提是有默认构造器), 入参 target 设为null;  
        /// 2. 将 source 的属性拷贝至 target, target 不能为空, 处理好的Target就是返回值（因为值类型作为参数时总是复制一份新的）
        /// </summary>
        /// <param name="sourceType"></param>
        /// <param name="targetType"></param>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        object Map(Type sourceType, Type targetType, object source, object target);
    }
}
