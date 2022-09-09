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
    public class DictionaryMapper<TSource, TTarget, TSourceKeyType, TSourceValueType, TTargetKeyType, TTargetValueType> : MapperBase
        where TSource : IDictionary<TSourceKeyType, TSourceValueType>
        where TTarget : IDictionary<TTargetKeyType, TTargetValueType>
        where TSourceKeyType : TTargetKeyType
    {
        private Type sourceKeyType;
        private Type sourceValueType;
        private Type targetKeyType;
        private Type targetValueType;

        public DictionaryMapper(IRootMapper rootMapper)
            : base(new TypePair(typeof(TSource), typeof(TTarget)), rootMapper)
        {
            sourceKeyType = typeof(TSourceKeyType);
            sourceValueType = typeof(TSourceValueType);
            targetKeyType = typeof(TTargetKeyType);
            targetValueType = typeof(TTargetValueType);
        }

        protected override object MapCore(object source, object target)
        {
            if (TypePair.SourceType == TypePair.TargetType)
            {
                return source;
            }
            if (source == null)
            {
                return target ?? default(TTarget);
            }

            return MapCore((IDictionary<TSourceKeyType, TSourceValueType>)source, (IDictionary<TTargetKeyType, TTargetValueType>)target);
        }


        private IDictionary<TTargetKeyType, TTargetValueType> MapCore(IDictionary<TSourceKeyType, TSourceValueType> sources, IDictionary<TTargetKeyType, TTargetValueType> targets)
        {
            if (targets != null)
            {
                targets.Clear();
            }
            else
            {
                targets = new Dictionary<TTargetKeyType, TTargetValueType>();
            }

            foreach (var kvp in sources)
            {
                var targetItemValue = RootMapper.Map(sourceValueType, targetValueType, kvp.Value, null);
                targets[kvp.Key] = (TTargetValueType)targetItemValue;
            }

            return targets;
        }
    }


    public class DictionaryMapperBuilder : MapperBuilderBase
    {
        public DictionaryMapperBuilder(IRootMapper rootMapper) : base(rootMapper)
        {
        }

        public override int Priority => 600;

        public override MapperBase Build(TypePair typePair)
        {
            if (TypeHelp.IsDictionaryOf(typePair.SourceType, out var sourceKeyType, out var sourceValueType)
                && TypeHelp.IsDictionaryOf(typePair.TargetType, out var targetKeyType, out var targetValueType))
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
