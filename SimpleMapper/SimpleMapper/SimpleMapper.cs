using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Collections.Concurrent;

namespace ZK
{
    public class SimpleMapper
    {
        public static void SetCustomMap<TSource, TTarget>(Func<TSource, TTarget> func)
        {
            var typePair = TypePair.Create<TSource, TTarget>();
            var typePairMapper = Container.Dicts.GetOrAdd(typePair, pair => new TypePairMapper(typeof(TSource), typeof(TTarget)));
            Func<object, object> objFunc = src =>
            {
                return func((TSource)src);
            };
            typePairMapper.SetCustomMap(objFunc);
        }

        public static void SetPostAction<TSource, TTarget>(Action<TSource, TTarget> action)
        {
            var typePair = TypePair.Create<TSource, TTarget>();
            var typePairMapper = Container.Dicts.GetOrAdd(typePair, pair => new TypePairMapper(typeof(TSource), typeof(TTarget)));
            Action<object, object> objAction = (src, dst) =>
            {
                action((TSource)src, (TTarget)dst);
            };
            typePairMapper.SetPostAction(objAction);
        }

        public static TTarget Map<TTarget, TSource>(TSource source)
            where TTarget : new()
        {
            return Map<TTarget>(source);
        }

        public static TTarget Map<TTarget>(object source)
            where TTarget : new()
        {
            if (source == null)
            {
                return default(TTarget);
            }

            Type sourceType = source.GetType();
            Type targetType = typeof(TTarget);
            var typePair = TypePair.Create(source.GetType(), typeof(TTarget));
            var typePairMapper = Container.Dicts.GetOrAdd(typePair, pair => new TypePairMapper(sourceType, targetType));

            var target = new TTarget();
            typePairMapper.Map(target, source);
            return target;
        }

        public static List<TTarget> Map<TTarget>(IEnumerable<object> sources)
            where TTarget : new()
        {
            if (sources == null)
            {
                return null;
            }

            var list = new List<TTarget>();
            foreach (var source in sources)
            {
                list.Add(Map<TTarget>(source));
            }
            return list;
        }

        public static void Inject<TTarget, TSource>(TTarget target, TSource source)
        {
            if (source == null || target == null)
            {
                return;
            }

            var typePair = TypePair.Create<TSource, TTarget>();
            var typePairMapper = Container.Dicts.GetOrAdd(typePair, pair => new TypePairMapper(typeof(TSource), typeof(TTarget)));

            typePairMapper.Map(target, source);
        }
    }



    public class TypePair
    {
        private int? hashCode;

        public TypePair(Type sourceType, Type targetType)
        {
            SourceType = sourceType;
            TargetType = targetType;
        }

        public Type SourceType { get; }

        public Type TargetType { get; }

        public override bool Equals(object obj)
        {
            var pair = obj as TypePair;
            if (pair == null)
            {
                return false;
            }
            return SourceType == pair.SourceType && TargetType == pair.TargetType;
        }

        public override int GetHashCode()
        {
            if (hashCode == null)
            {
                hashCode = SourceType.GetHashCode() & TargetType.GetHashCode();
            }
            return hashCode.Value;
        }

        public static TypePair Create<TTSource, TTarget>()
        {
            return new TypePair(typeof(TTSource), typeof(TTarget));
        }

        public static TypePair Create(Type sourceType, Type targetType)
        {
            return new TypePair(sourceType, targetType);
        }
    }



    internal class TypePairMapper
    {
        private Func<object, object> customMap;
        private Action<object, object> defaultMap;
        private Action<object, object> postAction;

        public TypePairMapper(Type sourceType, Type targetType)
        {
            TypePair = new TypePair(sourceType, targetType);
            var sourceMembers = (TypePair.SourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty).Select(o => new PropertyFieldInfo(o)))
                .Concat(TypePair.SourceType.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetField).Select(o => new PropertyFieldInfo(o))).ToList();
            var targetMembers = (TypePair.TargetType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty).Select(o => new PropertyFieldInfo(o)))
                .Concat(TypePair.TargetType.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetField).Select(o => new PropertyFieldInfo(o))).ToList();

            var sameNameTypes = new List<Tuple<PropertyFieldInfo, PropertyFieldInfo>>();
            var sameNameDifferentTypes = new List<Tuple<PropertyFieldInfo, PropertyFieldInfo>>();
            foreach (var sourceMember in sourceMembers)
            {
                var targetMember = targetMembers.Find(o => o.Name == sourceMember.Name);
                if (targetMember == null)
                {
                    continue;
                }
                if (targetMember.Type == sourceMember.Type)
                {
                    sameNameTypes.Add(Tuple.Create(sourceMember, targetMember));
                }
                else
                {
                    sameNameDifferentTypes.Add(Tuple.Create(sourceMember, targetMember));
                }
            }
            defaultMap = ExpressionGenerator.GenerateDefaultMap(TypePair.SourceType, TypePair.TargetType, sameNameTypes);
            SameNameSameTypes = sameNameTypes.Select(o => o.Item1.Name).ToArray();
            SameNameDifferentTypes = sameNameDifferentTypes.Select(o => o.Item1.Name).ToArray();
        }

        public string[] SameNameSameTypes { get; }

        public string[] SameNameDifferentTypes { get; }

        public TypePair TypePair { get; protected set; }


        public void SetCustomMap(Func<object, object> func)
        {
            this.customMap = func;
        }

        public void SetPostAction(Action<object, object> action)
        {
            this.postAction = action;
        }

        public virtual void Map(object target, object source)
        {
            if (customMap != null)
            {
                target = customMap(source);
            }
            else
            {
                if (source == null || target == null)
                {
                    return;
                }
                defaultMap(source, target);
            }

            if (postAction != null)
            {
                postAction(source, target);
            }
        }

        public override bool Equals(object obj)
        {
            var mapper = obj as TypePairMapper;
            if (mapper == null)
            {
                return false;
            }
            return TypePair.Equals(mapper.TypePair);
        }

        public override int GetHashCode()
        {
            return TypePair.GetHashCode();
        }
    }



    internal class PropertyFieldInfo
    {
        public PropertyFieldInfo(PropertyInfo propertyInfo)
        {
            PropertyInfo = propertyInfo;
            MemberType = MemberTypes.Property;
            Name = propertyInfo.Name;
            Type = propertyInfo.PropertyType;
        }

        public PropertyFieldInfo(FieldInfo fieldInfo)
        {
            FieldInfo = fieldInfo;
            MemberType = MemberTypes.Field;
            Name = fieldInfo.Name;
            Type = fieldInfo.FieldType;
        }

        public PropertyInfo PropertyInfo { get; set; }

        public FieldInfo FieldInfo { get; set; }

        public MemberTypes MemberType { get; set; }

        public string Name { get; set; }

        public Type Type { get; set; }
    }

    internal static class Container
    {
        public static readonly object Locker = new object();

        public static readonly ConcurrentDictionary<TypePair, TypePairMapper> Dicts = new ConcurrentDictionary<TypePair, TypePairMapper>();
    }

    internal static class ExpressionGenerator
    {
        public static readonly Type ObjectType = typeof(object);

        public static Action<object, object> GenerateDefaultMap(Type sourceType, Type targetType, List<Tuple<PropertyFieldInfo, PropertyFieldInfo>> memberTuples)
        {
            ParameterExpression objSrc = Expression.Parameter(ObjectType, "objSrc");
            ParameterExpression typedSrc = Expression.Variable(sourceType, "typedSrc");
            var srcConvert = Expression.Assign(typedSrc, Expression.Convert(objSrc, sourceType));  // Expression.Convert 强制类型转换

            ParameterExpression objDst = Expression.Parameter(ObjectType, "objDst");
            ParameterExpression typedDst = Expression.Variable(targetType, "typedDst");
            var dstConvert = Expression.Assign(typedDst, Expression.Convert(objDst, targetType));

            var binaryExpressions = new List<Expression>();
            binaryExpressions.Add(srcConvert);
            binaryExpressions.Add(dstConvert);
            foreach (var memberTuple in memberTuples)
            {
                MemberExpression left = (memberTuple.Item2.MemberType == MemberTypes.Property) ?
                    Expression.Property(typedDst, targetType.GetProperty(memberTuple.Item2.Name)) : Expression.Field(typedDst, targetType.GetField(memberTuple.Item2.Name));

                MemberExpression right = (memberTuple.Item1.MemberType == MemberTypes.Property) ?
                    Expression.Property(typedSrc, sourceType.GetProperty(memberTuple.Item1.Name)) : Expression.Field(typedSrc, sourceType.GetField(memberTuple.Item1.Name));

                binaryExpressions.Add(Expression.Assign(left, right)); // 赋值
            }
            // 坑死人, 定义变量 必须再套一个 block
            var block = Expression.Block(new[] { typedSrc, typedDst }, binaryExpressions);

            return Expression.Lambda<Action<object, object>>(block, new[] { objSrc, objDst }).Compile();
        }

        public static Action<object, object> Test(Type sourceType, Type targetType, string name)
        {
            var t = typeof(object);
            ParameterExpression leftBody = Expression.Parameter(t, "a");
            ParameterExpression rightBody = Expression.Parameter(t, "b");
            var leftP = targetType.GetProperty(name);
            var rightP = sourceType.GetProperty(name);

            var left = Expression.Property(leftBody, leftP);
            var right = Expression.Property(rightBody, rightP);
            var exp = Expression.Assign(left, right);
            return Expression.Lambda<Action<object, object>>(exp, new[] { rightBody, leftBody }).Compile();
        }
    }
}
