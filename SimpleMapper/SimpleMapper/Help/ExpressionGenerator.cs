using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using ZK.Mapper.Core;

namespace ZK.Mapper.Help
{
    internal static class ExpressionGenerator
    {
        /// <summary>
        /// 编译生成 默认构造器的创建方法, 这个比 直接代码慢一倍，但反射是直接代码的70倍时间
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Func<object> CreateEmptyCtor(Type type)
        {
            return Expression.Lambda<Func<object>>(Expression.New(type)).Compile();
        }

        /// <summary>
        /// 生成代码：将 TSource 里 同名同类型的属性拷贝至 Target
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TTarget"></typeparam>
        /// <param name="memberTuples"></param>
        /// <returns></returns>
        public static Action<TSource, TTarget> GenerateSameNameSameTypeCopy<TSource, TTarget>(List<Tuple<PropertyFieldInfo, PropertyFieldInfo>> memberTuples)
        {
            if (memberTuples.Count == 0)
            {
                return (source, target) => { };
            }

            Type sourceType = typeof(TSource);
            Type targetType = typeof(TTarget);

            ParameterExpression objSrc = Expression.Parameter(sourceType, "source");
            ParameterExpression objDst = Expression.Parameter(targetType, "target");

            var binaryExpressions = new List<Expression>();
            foreach (var memberTuple in memberTuples)
            {
                // 生成下面代码
                // target.targetMember = source.sourceMember
                MemberExpression left = memberTuple.Item2.MemberType == MemberTypes.Property ?
                    Expression.Property(objDst, targetType.GetProperty(memberTuple.Item2.Name)) : Expression.Field(objDst, targetType.GetField(memberTuple.Item2.Name));

                MemberExpression right = memberTuple.Item1.MemberType == MemberTypes.Property ?
                    Expression.Property(objSrc, sourceType.GetProperty(memberTuple.Item1.Name)) : Expression.Field(objSrc, sourceType.GetField(memberTuple.Item1.Name));

                binaryExpressions.Add(Expression.Assign(left, right)); // 赋值
            }

            var block = Expression.Block(binaryExpressions);
            return Expression.Lambda<Action<TSource, TTarget>>(block, new[] { objSrc, objDst }).Compile();
        }

        /// <summary>
        /// 生成代码：将 TSource 里 同名不同类型的属性 使用IRootMapper.Map转换后赋值给 Target
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TTarget"></typeparam>
        /// <param name="memberTuples"></param>
        /// <returns></returns>
        internal static Action<TSource, TTarget, IRootMapper> GenerateSameNameDifferentTypeCopy<TSource, TTarget>(List<Tuple<PropertyFieldInfo, PropertyFieldInfo>> memberTuples)
        {
            if (memberTuples.Count == 0)
            {
                return (source, target, rootMapper) => { };
            }

            Type sourceType = typeof(TSource);
            Type targetType = typeof(TTarget);
            Type rootMapperType = typeof(IRootMapper);

            ParameterExpression sourceParam = Expression.Parameter(sourceType, "source");
            ParameterExpression targetParam = Expression.Parameter(targetType, "target");
            ParameterExpression rootMapperParam = Expression.Parameter(rootMapperType, "rootMapper");
            // 获取 IRootMapper.Map 方法
            MethodInfo methodInfo = typeof(IRootMapper).GetMethod("Map", new Type[] { typeof(Type), typeof(Type), typeof(object), typeof(object) });

            var binaryExpressions = new List<Expression>();
            foreach (var memberTuple in memberTuples)
            {
                // 生成下面代码
                // target.targetMember = (targetMemberType)rootMapper.Map(sourceMemberType, targetMemberType, (object)source.sourceMember, (object)null)
                // target.Point1 = (Point)rootMapper.Map(typeof(Point?), typeof(Point), source.Point1, null)
                MemberExpression left = memberTuple.Item2.MemberType == MemberTypes.Property ?
                    Expression.Property(targetParam, targetType.GetProperty(memberTuple.Item2.Name)) : Expression.Field(targetParam, targetType.GetField(memberTuple.Item2.Name));
                MemberExpression sourceMember = memberTuple.Item1.MemberType == MemberTypes.Property ?
                    Expression.Property(sourceParam, sourceType.GetProperty(memberTuple.Item1.Name)) : Expression.Field(sourceParam, sourceType.GetField(memberTuple.Item1.Name));

                var sourceMemberType = Expression.Constant(memberTuple.Item1.Type, typeof(Type));
                var targetMemberType = Expression.Constant(memberTuple.Item2.Type, typeof(Type));
                var sourceObj = Expression.Convert(sourceMember, typeof(object));
                var targetObj = Expression.Constant(null, typeof(object));
                MethodCallExpression callExpression = Expression.Call(rootMapperParam, methodInfo, sourceMemberType, targetMemberType, sourceObj, targetObj);
                // 将 Map() 方法返回的结果强转为 targetMemberType
                var convertExpression = Expression.Convert(callExpression, memberTuple.Item2.Type);
                binaryExpressions.Add(Expression.Assign(left, convertExpression));
            }

            var block = Expression.Block(binaryExpressions);
            return Expression.Lambda<Action<TSource, TTarget, IRootMapper>>(block, new[] { sourceParam, targetParam, rootMapperParam }).Compile();
        }
    }
}
