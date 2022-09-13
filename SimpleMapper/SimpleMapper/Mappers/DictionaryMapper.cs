using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using ZK.Mapper.Core;
using ZK.Mapper.Help;

namespace ZK.Mapper.Mappers
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TTarget"></typeparam>
    /// <typeparam name="TSourceKeyType"></typeparam>
    /// <typeparam name="TSourceValueType"></typeparam>
    /// <typeparam name="TTargetKeyType"></typeparam>
    /// <typeparam name="TTargetValueType"></typeparam>
    internal class DictionaryMapper<TSource, TTarget, TSourceKeyType, TSourceValueType, TTargetKeyType, TTargetValueType> : MapperBase
        where TSource : IDictionary<TSourceKeyType, TSourceValueType>
        where TTarget : IDictionary<TTargetKeyType, TTargetValueType>
        where TSourceKeyType : TTargetKeyType
    {
        private static readonly Type sourceKeyType = typeof(TSourceKeyType);
        private static readonly Type sourceValueType = typeof(TSourceValueType);
        private static readonly Type targetKeyType = typeof(TTargetKeyType);
        private static readonly Type targetValueType = typeof(TTargetValueType);

        public DictionaryMapper(IRootMapper rootMapper)
            : base(new TypePair(typeof(TSource), typeof(TTarget)), rootMapper)
        {
        }

        protected override object MapCore(object source, object target, MapContext mapContext)
        {
            if (!mapContext.DeepCopy && TypePair.SourceType == TypePair.TargetType)
            {
                return source;
            }
            if (source == null)
            {
                return target ?? default(TTarget);
            }

            return MapCore((IDictionary<TSourceKeyType, TSourceValueType>)source, (IDictionary<TTargetKeyType, TTargetValueType>)target, mapContext);
        }


        private IDictionary<TTargetKeyType, TTargetValueType> MapCore(IDictionary<TSourceKeyType, TSourceValueType> sources, IDictionary<TTargetKeyType, TTargetValueType> targets, MapContext mapContext)
        {
            if (targets != null)
            {
                targets.Clear();
            }
            else
            {
                targets = new Dictionary<TTargetKeyType, TTargetValueType>();
            }

            var typePair = TypePair.Create<TSourceValueType, TTargetValueType>();
            MapperBase mapper = null;
            object targetItemValue;
            foreach (var kvp in sources)
            {
                if (mapper != null)
                {
                    targetItemValue = mapper.Map(kvp.Value, null, mapContext);
                }
                else
                {
                    targetItemValue = RootMapper.Map(sourceValueType, targetValueType, kvp.Value, null, mapContext);
                    mapper = RootMapper.MapperBaseDicts[typePair];
                }

                targets[kvp.Key] = (TTargetValueType)targetItemValue;
            }

            return targets;
        }
    }


    internal class DictionaryMapperBuilder : MapperBuilderBase
    {
        public DictionaryMapperBuilder(IRootMapper rootMapper) : base(rootMapper)
        {
        }

        public override int Priority => 600;

        public override MapperBase Build(TypePair typePair)
        {
            if (TypeHelp.IsDictionary(typePair.SourceType, out var sourceKeyType, out var sourceValueType)
                && TypeHelp.IsDictionary(typePair.TargetType, out var targetKeyType, out var targetValueType))
            {
                // Key 必须一样
                if (sourceKeyType == targetKeyType)
                {
                    var type = typeof(DictionaryMapper<,,,,,>).MakeGenericType(typePair.SourceType, typePair.TargetType, sourceKeyType, sourceValueType, targetKeyType, targetValueType);
                    var mapper = (MapperBase)type.GetConstructor(ImplementMapperConstructorTypes).Invoke(new object[] { RootMapper });
                    return mapper;
                }
            }

            return null;
        }
    }
}
