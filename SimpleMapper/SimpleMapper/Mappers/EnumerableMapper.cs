using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using ZK.Mapper.Core;
using ZK.Mapper.Help;

namespace ZK.Mapper.Mappers
{
    /// <summary>
    /// 数组到数组的拷贝，暂不支持 Dictionary
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TTarget"></typeparam>
    /// <typeparam name="TSourceItem"></typeparam>
    /// <typeparam name="TTargetItem"></typeparam>
    internal class EnumerableMapper<TSource, TTarget, TSourceItem, TTargetItem> : MapperBase
    {
        private static readonly Type sourceItemType = typeof(TSourceItem);
        private static readonly Type targetItemType = typeof(TTargetItem);

        private MapperType mapperType;

        public EnumerableMapper(IRootMapper rootMapper)
            : base(new TypePair(typeof(TSource), typeof(TTarget)), rootMapper)
        {
            if (TypeHelp.IsArrayOf(typeof(TTarget)))
            {
                mapperType = MapperType.EnumerableToArray;
            }
            else if (TypeHelp.IsListOf(typeof(TTarget)))
            {
                mapperType = MapperType.EnumerableToList;
            }
            else if (TypeHelp.IsCollectionOf(typeof(TTarget)))
            {
                mapperType = MapperType.EnumerableToCollection;
            }
            else if (TypeHelp.IsHashsetOf(typeof(TTarget)))
            {
                mapperType = MapperType.EnumerableToHashSet;
            }
            else
            {
                mapperType = MapperType.NotSupport;
            }
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

            switch (mapperType)
            {
                case MapperType.EnumerableToArray:
                    return EnumerableToArray((IEnumerable<TSourceItem>)source, (TTargetItem[])target, mapContext);
                case MapperType.EnumerableToList:
                    return EnumerableToList((IEnumerable<TSourceItem>)source, (List<TTargetItem>)target, mapContext);
                case MapperType.EnumerableToCollection:
                    return EnumerableToCollection((IEnumerable<TSourceItem>)source, (Collection<TTargetItem>)target, mapContext);
                case MapperType.EnumerableToHashSet:
                    return EnumerableToHashSet((IEnumerable<TSourceItem>)source, (HashSet<TTargetItem>)target, mapContext);
                case MapperType.NotSupport:
                default:
                    return target;
            }
        }

        private TTargetItem[] EnumerableToArray(IEnumerable<TSourceItem> sources, TTargetItem[] targets, MapContext mapContext)
        {
            var count = sources.Count();
            if (mapContext.DeepCopy)
            {
                targets = new TTargetItem[count];
            }
            else if (targets == null)
            {
                targets = new TTargetItem[count];
            }
            if (targets.Length < count)
            {
                count = targets.Length;
            }

            int index = 0;
            foreach (var source in sources)
            {
                if (index >= count)
                {
                    break;
                }
                var target = RootMapper.Map(sourceItemType, targetItemType, source, null, mapContext);
                targets[index] = (TTargetItem)target;
                index++;
            }
            return targets;
        }

        private List<TTargetItem> EnumerableToList(IEnumerable<TSourceItem> sources, List<TTargetItem> targets, MapContext mapContext)
        {
            if (targets != null)
            {
                targets.Clear();
            }
            else
            {
                targets = new List<TTargetItem>();
            }

            foreach (var source in sources)
            {
                var target = RootMapper.Map(sourceItemType, targetItemType, source, null, mapContext);
                targets.Add((TTargetItem)target);
            }

            return targets;
        }

        private Collection<TTargetItem> EnumerableToCollection(IEnumerable<TSourceItem> sources, Collection<TTargetItem> targets, MapContext mapContext)
        {
            if (targets != null)
            {
                targets.Clear();
            }
            else
            {
                targets = new Collection<TTargetItem>();
            }

            foreach (var source in sources)
            {
                var target = RootMapper.Map(sourceItemType, targetItemType, source, null, mapContext);
                targets.Add((TTargetItem)target);
            }

            return targets;
        }

        private HashSet<TTargetItem> EnumerableToHashSet(IEnumerable<TSourceItem> sources, HashSet<TTargetItem> targets, MapContext mapContext)
        {
            if (targets != null)
            {
                targets.Clear();
            }
            else
            {
                targets = new HashSet<TTargetItem>();
            }

            foreach (var source in sources)
            {
                var target = RootMapper.Map(sourceItemType, targetItemType, source, null, mapContext);
                if (target != null)
                {
                    targets.Add((TTargetItem)target);
                }
            }

            return targets;
        }


        private enum MapperType
        {
            NotSupport = 0,
            EnumerableToArray = 1,
            EnumerableToList = 2,
            EnumerableToCollection = 3,
            EnumerableToHashSet = 4,
        }
    }


    internal class EnumerableMapperBuilder : MapperBuilderBase
    {
        public EnumerableMapperBuilder(IRootMapper rootMapper) : base(rootMapper)
        {
        }

        public override int Priority => 500;

        public override MapperBase Build(TypePair typePair)
        {
            if (TypeHelp.IsEnumerable(typePair.SourceType, out var sourceItemType) && TypeHelp.IsEnumerable(typePair.TargetType, out var targetItemType))
            {
                var type = typeof(EnumerableMapper<,,,>).MakeGenericType(typePair.SourceType, typePair.TargetType, sourceItemType, targetItemType);
                var mapper = (MapperBase)type.GetConstructor(ImplementMapperConstructorTypes).Invoke(new object[] { RootMapper });
                return mapper;
            }

            return null;
        }
    }
}
