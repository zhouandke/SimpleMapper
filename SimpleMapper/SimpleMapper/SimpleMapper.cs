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
                new DictionaryMapperBuilder(this),
                new EnumerableMapperBuilder(this),
                new ToStringMapperBuilder(this),
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
            return (TTarget)target;
        }

        public object Map(Type sourceType, Type targetType, object source, object target)
        {
            var typePair = TypePair.Create(sourceType, targetType);
            var typePairMapper = MapperBaseDicts.GetOrAdd(typePair, pair => CreateMapper(pair));
            target = typePairMapper.Map(source, target);
            return target;
        }

        public TTarget InjectFrom<TTarget, TSource>(TTarget target, TSource source)
        {
            if (source == null || target == null)
            {
                return target;
            }

            var typePair = TypePair.Create<TSource, TTarget>();
            var typePairMapper = MapperBaseDicts.GetOrAdd(typePair, pair => CreateMapper(pair));
            target = (TTarget)typePairMapper.Map(source, target);
            return target;
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
            SetCustomMap<string, bool>(src => bool.TryParse(src, out var value) ? value : default);
            SetCustomMap<string, byte>(src => byte.TryParse(src, out var value) ? value : default);
            SetCustomMap<string, sbyte>(src => sbyte.TryParse(src, out var value) ? value : default);
            SetCustomMap<string, short>(src => short.TryParse(src, out var value) ? value : default);
            SetCustomMap<string, ushort>(src => ushort.TryParse(src, out var value) ? value : default);
            SetCustomMap<string, int>(src => int.TryParse(src, out var value) ? value : default);
            SetCustomMap<string, uint>(src => uint.TryParse(src, out var value) ? value : default);
            SetCustomMap<string, long>(src => long.TryParse(src, out var value) ? value : default);
            SetCustomMap<string, ulong>(src => ulong.TryParse(src, out var value) ? value : default);
            SetCustomMap<string, float>(src => float.TryParse(src, out var value) ? value : default);
            SetCustomMap<string, double>(src => double.TryParse(src, out var value) ? value : default);
            SetCustomMap<string, decimal>(src => decimal.TryParse(src, out var value) ? value : default);
            SetCustomMap<string, DateTime>(src => DateTime.TryParse(src, out var value) ? value : default);
            SetCustomMap<string, TimeSpan>(src => TimeSpan.TryParse(src, out var value) ? value : default);

            SetCustomMap<string, bool?>(src => bool.TryParse(src, out var value) ? (bool?)value : default);
            SetCustomMap<string, byte?>(src => byte.TryParse(src, out var value) ? (byte?)value : default);
            SetCustomMap<string, sbyte?>(src => sbyte.TryParse(src, out var value) ? (sbyte?)value : default);
            SetCustomMap<string, short?>(src => short.TryParse(src, out var value) ? (short?)value : default);
            SetCustomMap<string, ushort?>(src => ushort.TryParse(src, out var value) ? (ushort?)value : default);
            SetCustomMap<string, int?>(src => int.TryParse(src, out var value) ? (int?)value : default);
            SetCustomMap<string, uint?>(src => uint.TryParse(src, out var value) ? (uint?)value : default);
            SetCustomMap<string, long?>(src => long.TryParse(src, out var value) ? (long?)value : default);
            SetCustomMap<string, ulong?>(src => ulong.TryParse(src, out var value) ? (ulong?)value : default);
            SetCustomMap<string, float?>(src => float.TryParse(src, out var value) ? (float?)value : default);
            SetCustomMap<string, double?>(src => double.TryParse(src, out var value) ? (double?)value : default);
            SetCustomMap<string, decimal?>(src => decimal.TryParse(src, out var value) ? (decimal?)value : default);
            SetCustomMap<string, DateTime?>(src => DateTime.TryParse(src, out var value) ? (DateTime?)value : default);
            SetCustomMap<string, TimeSpan?>(src => TimeSpan.TryParse(src, out var value) ? (TimeSpan?)value : default);
        }
    }
}
