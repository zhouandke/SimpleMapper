using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;

namespace ZK
{
    public class EnumerableMapper<TSource, TTarget, TSourceItem, TTargetItem> : MapperBase
    {
        private Type sourceItemType;
        private Type targetItemType;
        private MapperType mapperType;


        public EnumerableMapper(IRootMapper rootMapper)
            : base(new TypePair(typeof(TSource), typeof(TTarget)), rootMapper)
        {
            sourceItemType = typeof(TSourceItem);
            targetItemType = typeof(TTargetItem);
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

        protected override object MapCore(object source, object target)
        {
            if (TypePair.SourceType == TypePair.TargetType)
            {
                return source;
            }
            if (source == null)
            {
                return default(TTarget);
            }
            switch (mapperType)
            {
                case MapperType.EnumerableToArray:
                    return EnumerableToArray((IEnumerable<TSourceItem>)source, (TTargetItem[])target);
                case MapperType.EnumerableToList:
                    return EnumerableToList((IEnumerable<TSourceItem>)source, (List<TTargetItem>)target);
                case MapperType.EnumerableToCollection:
                    return EnumerableToCollection((IEnumerable<TSourceItem>)source, (Collection<TTargetItem>)target);
                case MapperType.EnumerableToHashSet:
                    return EnumerableToHashSet((IEnumerable<TSourceItem>)source, (HashSet<TTargetItem>)target);
                case MapperType.NotSupport:
                default:
                    return target;
            }
        }

        private TTargetItem[] EnumerableToArray(IEnumerable<TSourceItem> sources, TTargetItem[] targets)
        {
            var count = sources.Count();
            if (targets != null)
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
                var target = RootMapper.Map(sourceItemType, targetItemType, source);
                targets[index] = (TTargetItem)target;
                index++;
            }
            return targets;
        }

        private List<TTargetItem> EnumerableToList(IEnumerable<TSourceItem> sources, List<TTargetItem> targets)
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
                var target = RootMapper.Map(sourceItemType, targetItemType, source);
                targets.Add((TTargetItem)target);
            }

            return targets;
        }

        private Collection<TTargetItem> EnumerableToCollection(IEnumerable<TSourceItem> sources, Collection<TTargetItem> targets)
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
                var target = RootMapper.Map(sourceItemType, targetItemType, source);
                targets.Add((TTargetItem)target);
            }

            return targets;
        }

        private HashSet<TTargetItem> EnumerableToHashSet(IEnumerable<TSourceItem> sources, HashSet<TTargetItem> targets)
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
                var target = RootMapper.Map(sourceItemType, targetItemType, source);
                targets.Add((TTargetItem)target);
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
}
