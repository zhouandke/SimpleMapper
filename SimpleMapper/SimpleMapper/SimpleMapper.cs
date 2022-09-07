using System;
using System.Collections.Concurrent;

namespace ZK
{
    public class SimpleMapper : IRootMapper
    {
        public static readonly SimpleMapper Default = new SimpleMapper();

        private readonly Type[] implementMapperConstructorTypes; // mapper实现类的构造器要传入的 type
        private readonly object[] implementMapperConstructorObjects; // mapper实现类的构造器要传入的 参数
        private readonly object[] emptyObjects;

        public SimpleMapper()
        {
            implementMapperConstructorTypes = new Type[] { typeof(IRootMapper) };
            implementMapperConstructorObjects = new object[] { this };
            emptyObjects = new object[0];
            //AddBasicMaps();
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
                return default(TTarget);
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
            if (TypeHelp.IsPrimitiveNullable(typePair.SourceType, out var sourcePrimitveType)
                && TypeHelp.IsPrimitiveNullable(typePair.TargetType, out var targetPrimitveType))
            {
                var type = typeof(PrimitiveTypeMapper<,,,>).MakeGenericType(typePair.SourceType, typePair.TargetType, sourcePrimitveType, targetPrimitveType);
                var mapper = (MapperBase)type.GetConstructor(implementMapperConstructorTypes).Invoke(implementMapperConstructorObjects);
                return mapper;
            }

            if (TypeHelp.IsIEnumerableOf(typePair.SourceType) && TypeHelp.IsIEnumerableOf(typePair.TargetType))
            {
                var sourceItemType = TypeHelp.GetEnumerableItemType(typePair.SourceType);
                var targetItemType = TypeHelp.GetEnumerableItemType(typePair.TargetType);
                var type = typeof(EnumerableMapper<,,,>).MakeGenericType(typePair.SourceType, typePair.TargetType, sourceItemType, targetItemType);
                var mapper = (MapperBase)type.GetConstructor(implementMapperConstructorTypes).Invoke(implementMapperConstructorObjects);
                return mapper;
            }
            else
            {
                var type = typeof(ClassMapper<,>).MakeGenericType(typePair.SourceType, typePair.TargetType);
                var mapper = (MapperBase)type.GetConstructor(implementMapperConstructorTypes).Invoke(implementMapperConstructorObjects);
                return mapper;
            }
        }

        private void AddBasicMaps()
        {
            SetCustomMap<bool, bool>(src => src);
            SetCustomMap<bool, char>(src => (char)(src ? 1 : 0));
            SetCustomMap<bool, sbyte>(src => (sbyte)(src ? 1 : 0));
            SetCustomMap<bool, byte>(src => (byte)(src ? 1 : 0));
            SetCustomMap<bool, short>(src => (short)(src ? 1 : 0));
            SetCustomMap<bool, ushort>(src => (ushort)(src ? 1 : 0));
            SetCustomMap<bool, int>(src => (int)(src ? 1 : 0));
            SetCustomMap<bool, uint>(src => (uint)(src ? 1 : 0));
            SetCustomMap<bool, long>(src => (long)(src ? 1 : 0));
            SetCustomMap<bool, ulong>(src => (ulong)(src ? 1 : 0));
            SetCustomMap<bool, float>(src => (float)(src ? 1 : 0));
            SetCustomMap<bool, double>(src => (double)(src ? 1 : 0));
            SetCustomMap<bool, decimal>(src => (decimal)(src ? 1 : 0));
            SetCustomMap<bool, string>(src => src.ToString());

            SetCustomMap<char, bool>(src => src != 0);
            SetCustomMap<char, char>(src => (char)src);
            SetCustomMap<char, sbyte>(src => (sbyte)src);
            SetCustomMap<char, byte>(src => (byte)src);
            SetCustomMap<char, short>(src => (short)src);
            SetCustomMap<char, ushort>(src => (ushort)src);
            SetCustomMap<char, int>(src => (int)src);
            SetCustomMap<char, uint>(src => (uint)src);
            SetCustomMap<char, long>(src => (long)src);
            SetCustomMap<char, ulong>(src => (ulong)src);
            SetCustomMap<char, float>(src => (float)src);
            SetCustomMap<char, double>(src => (double)src);
            SetCustomMap<char, decimal>(src => (decimal)src);
            SetCustomMap<char, string>(src => src.ToString());

            SetCustomMap<sbyte, bool>(src => src != 0);
            SetCustomMap<sbyte, char>(src => (char)src);
            SetCustomMap<sbyte, sbyte>(src => (sbyte)src);
            SetCustomMap<sbyte, byte>(src => (byte)src);
            SetCustomMap<sbyte, short>(src => (short)src);
            SetCustomMap<sbyte, ushort>(src => (ushort)src);
            SetCustomMap<sbyte, int>(src => (int)src);
            SetCustomMap<sbyte, uint>(src => (uint)src);
            SetCustomMap<sbyte, long>(src => (long)src);
            SetCustomMap<sbyte, ulong>(src => (ulong)src);
            SetCustomMap<sbyte, float>(src => (float)src);
            SetCustomMap<sbyte, double>(src => (double)src);
            SetCustomMap<sbyte, decimal>(src => (decimal)src);
            SetCustomMap<sbyte, string>(src => src.ToString());

            SetCustomMap<byte, bool>(src => src != 0);
            SetCustomMap<byte, char>(src => (char)src);
            SetCustomMap<byte, sbyte>(src => (sbyte)src);
            SetCustomMap<byte, byte>(src => (byte)src);
            SetCustomMap<byte, short>(src => (short)src);
            SetCustomMap<byte, ushort>(src => (ushort)src);
            SetCustomMap<byte, int>(src => (int)src);
            SetCustomMap<byte, uint>(src => (uint)src);
            SetCustomMap<byte, long>(src => (long)src);
            SetCustomMap<byte, ulong>(src => (ulong)src);
            SetCustomMap<byte, float>(src => (float)src);
            SetCustomMap<byte, double>(src => (double)src);
            SetCustomMap<byte, decimal>(src => (decimal)src);
            SetCustomMap<byte, string>(src => src.ToString());

            SetCustomMap<short, bool>(src => src != 0);
            SetCustomMap<short, char>(src => (char)src);
            SetCustomMap<short, sbyte>(src => (sbyte)src);
            SetCustomMap<short, byte>(src => (byte)src);
            SetCustomMap<short, short>(src => (short)src);
            SetCustomMap<short, ushort>(src => (ushort)src);
            SetCustomMap<short, int>(src => (int)src);
            SetCustomMap<short, uint>(src => (uint)src);
            SetCustomMap<short, long>(src => (long)src);
            SetCustomMap<short, ulong>(src => (ulong)src);
            SetCustomMap<short, float>(src => (float)src);
            SetCustomMap<short, double>(src => (double)src);
            SetCustomMap<short, decimal>(src => (decimal)src);
            SetCustomMap<short, string>(src => src.ToString());

            SetCustomMap<ushort, bool>(src => src != 0);
            SetCustomMap<ushort, char>(src => (char)src);
            SetCustomMap<ushort, sbyte>(src => (sbyte)src);
            SetCustomMap<ushort, byte>(src => (byte)src);
            SetCustomMap<ushort, short>(src => (short)src);
            SetCustomMap<ushort, ushort>(src => (ushort)src);
            SetCustomMap<ushort, int>(src => (int)src);
            SetCustomMap<ushort, uint>(src => (uint)src);
            SetCustomMap<ushort, long>(src => (long)src);
            SetCustomMap<ushort, ulong>(src => (ulong)src);
            SetCustomMap<ushort, float>(src => (float)src);
            SetCustomMap<ushort, double>(src => (double)src);
            SetCustomMap<ushort, decimal>(src => (decimal)src);
            SetCustomMap<ushort, string>(src => src.ToString());

            SetCustomMap<int, bool>(src => src != 0);
            SetCustomMap<int, char>(src => (char)src);
            SetCustomMap<int, sbyte>(src => (sbyte)src);
            SetCustomMap<int, byte>(src => (byte)src);
            SetCustomMap<int, short>(src => (short)src);
            SetCustomMap<int, ushort>(src => (ushort)src);
            SetCustomMap<int, int>(src => (int)src);
            SetCustomMap<int, uint>(src => (uint)src);
            SetCustomMap<int, long>(src => (long)src);
            SetCustomMap<int, ulong>(src => (ulong)src);
            SetCustomMap<int, float>(src => (float)src);
            SetCustomMap<int, double>(src => (double)src);
            SetCustomMap<int, decimal>(src => (decimal)src);
            SetCustomMap<int, string>(src => src.ToString());

            SetCustomMap<uint, bool>(src => src != 0);
            SetCustomMap<uint, char>(src => (char)src);
            SetCustomMap<uint, sbyte>(src => (sbyte)src);
            SetCustomMap<uint, byte>(src => (byte)src);
            SetCustomMap<uint, short>(src => (short)src);
            SetCustomMap<uint, ushort>(src => (ushort)src);
            SetCustomMap<uint, int>(src => (int)src);
            SetCustomMap<uint, uint>(src => (uint)src);
            SetCustomMap<uint, long>(src => (long)src);
            SetCustomMap<uint, ulong>(src => (ulong)src);
            SetCustomMap<uint, float>(src => (float)src);
            SetCustomMap<uint, double>(src => (double)src);
            SetCustomMap<uint, decimal>(src => (decimal)src);
            SetCustomMap<uint, string>(src => src.ToString());

            SetCustomMap<long, bool>(src => src != 0);
            SetCustomMap<long, char>(src => (char)src);
            SetCustomMap<long, sbyte>(src => (sbyte)src);
            SetCustomMap<long, byte>(src => (byte)src);
            SetCustomMap<long, short>(src => (short)src);
            SetCustomMap<long, ushort>(src => (ushort)src);
            SetCustomMap<long, int>(src => (int)src);
            SetCustomMap<long, uint>(src => (uint)src);
            SetCustomMap<long, long>(src => (long)src);
            SetCustomMap<long, ulong>(src => (ulong)src);
            SetCustomMap<long, float>(src => (float)src);
            SetCustomMap<long, double>(src => (double)src);
            SetCustomMap<long, decimal>(src => (decimal)src);
            SetCustomMap<long, string>(src => src.ToString());

            SetCustomMap<ulong, bool>(src => src != 0);
            SetCustomMap<ulong, char>(src => (char)src);
            SetCustomMap<ulong, sbyte>(src => (sbyte)src);
            SetCustomMap<ulong, byte>(src => (byte)src);
            SetCustomMap<ulong, short>(src => (short)src);
            SetCustomMap<ulong, ushort>(src => (ushort)src);
            SetCustomMap<ulong, int>(src => (int)src);
            SetCustomMap<ulong, uint>(src => (uint)src);
            SetCustomMap<ulong, long>(src => (long)src);
            SetCustomMap<ulong, ulong>(src => (ulong)src);
            SetCustomMap<ulong, float>(src => (float)src);
            SetCustomMap<ulong, double>(src => (double)src);
            SetCustomMap<ulong, decimal>(src => (decimal)src);
            SetCustomMap<ulong, string>(src => src.ToString());

            SetCustomMap<float, bool>(src => src != 0);
            SetCustomMap<float, char>(src => (char)src);
            SetCustomMap<float, sbyte>(src => (sbyte)src);
            SetCustomMap<float, byte>(src => (byte)src);
            SetCustomMap<float, short>(src => (short)src);
            SetCustomMap<float, ushort>(src => (ushort)src);
            SetCustomMap<float, int>(src => (int)src);
            SetCustomMap<float, uint>(src => (uint)src);
            SetCustomMap<float, long>(src => (long)src);
            SetCustomMap<float, ulong>(src => (ulong)src);
            SetCustomMap<float, float>(src => (float)src);
            SetCustomMap<float, double>(src => (double)src);
            SetCustomMap<float, decimal>(src => (decimal)src);
            SetCustomMap<float, string>(src => src.ToString());

            SetCustomMap<double, bool>(src => src != 0);
            SetCustomMap<double, char>(src => (char)src);
            SetCustomMap<double, sbyte>(src => (sbyte)src);
            SetCustomMap<double, byte>(src => (byte)src);
            SetCustomMap<double, short>(src => (short)src);
            SetCustomMap<double, ushort>(src => (ushort)src);
            SetCustomMap<double, int>(src => (int)src);
            SetCustomMap<double, uint>(src => (uint)src);
            SetCustomMap<double, long>(src => (long)src);
            SetCustomMap<double, ulong>(src => (ulong)src);
            SetCustomMap<double, float>(src => (float)src);
            SetCustomMap<double, double>(src => (double)src);
            SetCustomMap<double, decimal>(src => (decimal)src);
            SetCustomMap<double, string>(src => src.ToString());

            SetCustomMap<decimal, bool>(src => src != 0);
            SetCustomMap<decimal, char>(src => (char)src);
            SetCustomMap<decimal, sbyte>(src => (sbyte)src);
            SetCustomMap<decimal, byte>(src => (byte)src);
            SetCustomMap<decimal, short>(src => (short)src);
            SetCustomMap<decimal, ushort>(src => (ushort)src);
            SetCustomMap<decimal, int>(src => (int)src);
            SetCustomMap<decimal, uint>(src => (uint)src);
            SetCustomMap<decimal, long>(src => (long)src);
            SetCustomMap<decimal, ulong>(src => (ulong)src);
            SetCustomMap<decimal, float>(src => (float)src);
            SetCustomMap<decimal, double>(src => (double)src);
            SetCustomMap<decimal, decimal>(src => (decimal)src);
            SetCustomMap<decimal, string>(src => src.ToString());

            SetCustomMap<DateTime, string>(src => src.ToString());
            SetCustomMap<string, DateTime>(src => { DateTime value; return DateTime.TryParse(src, out value) ? value : default(DateTime); });
        }
    }
}
