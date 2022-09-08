using System;
using System.Collections.Concurrent;
using System.Linq;
using ZK.Mapper.Core;
using ZK.Mapper.Mappers;

namespace ZK.Mapper
{
    public class SimpleMapper : IRootMapper
    {
        public static readonly SimpleMapper Default = new SimpleMapper();

        private readonly MapperBuilderBase[] mapperBuilders;

        public SimpleMapper()
        {
            mapperBuilders = new MapperBuilderBase[]
            {
                new PrimitiveTypeMapperBuilder(this),
                new NullableMapperBuilder(this),
                new EnumerableMapperBuilder(this),
                new ClassMapperBuilder(this),
            }
            .OrderByDescending(o => o.Priority)
            .ToArray();

            AddBasicMaps();
        }


        public ConcurrentDictionary<TypePair, MapperBase> MapperBaseDicts { get; } = new ConcurrentDictionary<TypePair, MapperBase>();

        public ConcurrentDictionary<TypePair, Action<object, object>> PostActionDicts { get; } = new ConcurrentDictionary<TypePair, Action<object, object>>();

        public void SetCustomMap<TSource, TTarget>(Func<TSource, TTarget> func)
        {
            var typePair = TypePair.Create<TSource, TTarget>();
            var mapper = new CustomMapper<TSource, TTarget>((source, target) => func(source), this);
            MapperBaseDicts[typePair] = mapper;
        }

        public void SetCustomMap<TSource, TTarget>(Func<TSource, TTarget, TTarget> func)
        {
            var typePair = TypePair.Create<TSource, TTarget>();
            var mapper = new CustomMapper<TSource, TTarget>(func, this);
            MapperBaseDicts[typePair] = mapper;
        }

        public void SetPostAction<TSource, TTarget>(Action<TSource, TTarget> action)
        {
            var typePair = TypePair.Create<TSource, TTarget>();
            PostActionDicts[typePair] = (sourcObj, targetObj) => action((TSource)sourcObj, (TTarget)targetObj);
        }

        public TTarget Map<TTarget, TSource>(TSource source)
            where TTarget : new()
        {
            return Map<TTarget>(source);
        }

        public TTarget Map<TTarget>(object source)
            where TTarget : new()
        {
            if (source == null)
            {
                return default;
            }

            var typePair = TypePair.Create(source.GetType(), typeof(TTarget));
            var typePairMapper = MapperBaseDicts.GetOrAdd(typePair, pair => CreateMapper(pair));

            var target = new TTarget();
            target = (TTarget)typePairMapper.Map(source, target);
            return target;
        }

        public object Map(Type sourceType, Type targetType, object source, object target = null)
        {
            var typePair = TypePair.Create(sourceType, targetType);
            var typePairMapper = MapperBaseDicts.GetOrAdd(typePair, pair => CreateMapper(pair));
            target = typePairMapper.Map(source, target);
            return target;
        }

        public void Inject<TTarget, TSource>(TTarget target, TSource source)
        {
            if (source == null || target == null)
            {
                return;
            }

            var typePair = TypePair.Create<TSource, TTarget>();
            var typePairMapper = MapperBaseDicts.GetOrAdd(typePair, pair => CreateMapper(pair));

            typePairMapper.Map(source, target);
        }

        private MapperBase CreateMapper(TypePair typePair)
        {
            for (int i = 0; i < mapperBuilders.Length; i++)
            {
                var mapper = mapperBuilders[i].Build(typePair);
                if (mapper != null)
                {
                    return mapper;
                }
            }
            throw new NotSupportedException($"No Mapper support {typePair.SourceType.Name} to {typePair.TargetType.Name}");
        }

        private void AddBasicMaps()
        {
            SetCustomMap<DateTime, string>(src => src.ToString());
            SetCustomMap<string, DateTime>(src => { DateTime value; return DateTime.TryParse(src, out value) ? value : default; });
        }
    }
}
