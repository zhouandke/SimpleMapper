using System;

namespace ZK.Mapper.Core
{
    public abstract class MapperBuilderBase
    {
        /// <summary>
        /// mapper实现类的构造器要传入的 type
        /// </summary>
        protected readonly Type[] ImplementMapperConstructorTypes = new Type[] { typeof(IRootMapper) };

        protected MapperBuilderBase(IRootMapper rootMapper)
        {
            RootMapper = rootMapper;
        }

        protected IRootMapper RootMapper { get; }

        /// <summary>
        /// 使用当前 Builder 的优先级, 值越大越优先使用
        /// </summary>
        public abstract int Priority { get; }

        public abstract MapperBase Build(TypePair typePair);
    }
}
