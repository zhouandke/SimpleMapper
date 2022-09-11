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
                new EnumMapperBuilder(this),
                new NullableMapperBuilder(this),
                new DictionaryMapperBuilder(this),
                new EnumerableMapperBuilder(this),
                new ToStringMapperBuilder(this),
                new BasicMapperBuilder(this),
            }
            .OrderByDescending(o => o.Priority)
            .ToArray();

            AddBasicMaps();
        }

        public ConcurrentDictionary<TypePair, MapperBase> MapperBaseDicts { get; } = new ConcurrentDictionary<TypePair, MapperBase>();

        public ConcurrentDictionary<TypePair, Func<object, object, object>> PostActionDicts { get; } = new ConcurrentDictionary<TypePair, Func<object, object, object>>();

        public ImmutableTypeManage ImmutableTypeManage { get; } = new ImmutableTypeManage();

        /// <summary>
        /// 设定某个 Type 为不可变类型
        /// 这个主要影响深度拷贝时, 某个 Type 是否直接拷贝
        /// 默认是: 没有可写属性, 也没有任何公开字段, 就认为是不可变类型
        /// </summary>
        /// <param name="type"></param>
        public void SetImmutableType(Type type, bool isImmutable)
        {
            ImmutableTypeManage.RegisterImmutableType(type, isImmutable);
        }

        [Obsolete("改方法设置的mapper, 不能用于 InjectFrom")]
        public void SetCustomMap<TSource, TTarget>(Func<TSource, TTarget> func)
        {
            var typePair = TypePair.Create<TSource, TTarget>();
            var mapper = new CustomMapper<TSource, TTarget>((source, target) => func(source), this);
            MapperBaseDicts[typePair] = mapper;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TTarget"></typeparam>
        /// <param name="func">Pay attention, parameter target may be null</param>
        public void SetCustomMap<TSource, TTarget>(Func<TSource, TTarget, TTarget> func)
        {
            var typePair = TypePair.Create<TSource, TTarget>();
            var mapper = new CustomMapper<TSource, TTarget>(func, this);
            MapperBaseDicts[typePair] = mapper;
        }

        public void SetPostAction<TSource, TTarget>(Func<TSource, TTarget, TTarget> func)
        {
            var typePair = TypePair.Create<TSource, TTarget>();
            PostActionDicts[typePair] = (sourcObj, targetObj) => func((TSource)sourcObj, (TTarget)targetObj);
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
            var mapperBase = MapperBaseDicts.GetOrAdd(typePair, pair => CreateMapper(pair));

            var target = new TTarget();
            target = (TTarget)mapperBase.Map(source, target, new MapContext());
            return (TTarget)target;
        }

        public TTarget MapAsDeepCopy<TTarget>(object source)
            where TTarget : new()
        {
            if (source == null)
            {
                return default;
            }

            var typePair = TypePair.Create(source.GetType(), typeof(TTarget));
            var mapperBase = MapperBaseDicts.GetOrAdd(typePair, pair => CreateMapper(pair));

            var target = new TTarget();
            target = (TTarget)mapperBase.Map(source, target, new MapContext(true));
            return (TTarget)target;
        }

        public object Map(Type sourceType, Type targetType, object source, object target, MapContext mapContext)
        {
            var typePair = TypePair.Create(sourceType, targetType);
            var mapperBase = MapperBaseDicts.GetOrAdd(typePair, pair => CreateMapper(pair));
            target = mapperBase.Map(source, target, mapContext);
            return target;
        }

        public TTarget InjectFrom<TTarget, TSource>(TTarget target, TSource source)
        {
            if (source == null || target == null)
            {
                return target;
            }

            var typePair = TypePair.Create<TSource, TTarget>();
            var mapperBase = MapperBaseDicts.GetOrAdd(typePair, pair => CreateMapper(pair));
            target = (TTarget)mapperBase.Map(source, target, new MapContext());
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
            SetCustomMap<string, bool>((src, _) => bool.TryParse(src, out var value) ? value : default);
            SetCustomMap<string, byte>((src, _) => byte.TryParse(src, out var value) ? value : default);
            SetCustomMap<string, sbyte>((src, _) => sbyte.TryParse(src, out var value) ? value : default);
            SetCustomMap<string, char>((src, _) => char.TryParse(src, out var value) ? value : default);
            SetCustomMap<string, short>((src, _) => short.TryParse(src, out var value) ? value : default);
            SetCustomMap<string, ushort>((src, _) => ushort.TryParse(src, out var value) ? value : default);
            SetCustomMap<string, int>((src, _) => int.TryParse(src, out var value) ? value : default);
            SetCustomMap<string, uint>((src, _) => uint.TryParse(src, out var value) ? value : default);
            SetCustomMap<string, long>((src, _) => long.TryParse(src, out var value) ? value : default);
            SetCustomMap<string, ulong>((src, _) => ulong.TryParse(src, out var value) ? value : default);
            SetCustomMap<string, float>((src, _) => float.TryParse(src, out var value) ? value : default);
            SetCustomMap<string, double>((src, _) => double.TryParse(src, out var value) ? value : default);
            SetCustomMap<string, decimal>((src, _) => decimal.TryParse(src, out var value) ? value : default);
            SetCustomMap<string, DateTime>((src, _) => DateTime.TryParse(src, out var value) ? value : default);
            SetCustomMap<string, TimeSpan>((src, _) => TimeSpan.TryParse(src, out var value) ? value : default);

            SetCustomMap<string, bool?>((src, _) => bool.TryParse(src, out var value) ? (bool?)value : default);
            SetCustomMap<string, byte?>((src, _) => byte.TryParse(src, out var value) ? (byte?)value : default);
            SetCustomMap<string, sbyte?>((src, _) => sbyte.TryParse(src, out var value) ? (sbyte?)value : default);
            SetCustomMap<string, char?>((src, _) => char.TryParse(src, out var value) ? (char?)value : default);
            SetCustomMap<string, short?>((src, _) => short.TryParse(src, out var value) ? (short?)value : default);
            SetCustomMap<string, ushort?>((src, _) => ushort.TryParse(src, out var value) ? (ushort?)value : default);
            SetCustomMap<string, int?>((src, _) => int.TryParse(src, out var value) ? (int?)value : default);
            SetCustomMap<string, uint?>((src, _) => uint.TryParse(src, out var value) ? (uint?)value : default);
            SetCustomMap<string, long?>((src, _) => long.TryParse(src, out var value) ? (long?)value : default);
            SetCustomMap<string, ulong?>((src, _) => ulong.TryParse(src, out var value) ? (ulong?)value : default);
            SetCustomMap<string, float?>((src, _) => float.TryParse(src, out var value) ? (float?)value : default);
            SetCustomMap<string, double?>((src, _) => double.TryParse(src, out var value) ? (double?)value : default);
            SetCustomMap<string, decimal?>((src, _) => decimal.TryParse(src, out var value) ? (decimal?)value : default);
            SetCustomMap<string, DateTime?>((src, _) => DateTime.TryParse(src, out var value) ? (DateTime?)value : default);
            SetCustomMap<string, TimeSpan?>((src, _) => TimeSpan.TryParse(src, out var value) ? (TimeSpan?)value : default);
        }
    }
}
