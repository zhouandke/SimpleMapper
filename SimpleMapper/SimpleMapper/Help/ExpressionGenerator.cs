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
        /// 编译生成 无参构造器的创建方法, 直接 new 耗时1, 该方法是 2, Activator.CreateInstance 是 3, 反射是70
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Func<object> CreateParameterlessCtor(Type type)
        {
            if (type.IsClass)
            {
                return Expression.Lambda<Func<object>>(Expression.New(type)).Compile();
            }
            else
            {
                return () => Activator.CreateInstance(type);  
            }
        }

        /// <summary>
        /// 生成代码：将 TSource 里 同名同类型的属性拷贝至 Target
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TTarget"></typeparam>
        /// <param name="memberPairs"></param>
        /// <returns></returns>
        public static Func<TSource, TTarget, TTarget> GenerateSameNameSameTypeCopy<TSource, TTarget>(List<SourceTargetMemberPair> memberPairs)
        {
            if (memberPairs.Count == 0)
            {
                return (source, target) => target;
            }

            Type sourceType = typeof(TSource);
            Type targetType = typeof(TTarget);

            ParameterExpression sourceParam = Expression.Parameter(sourceType, "source");
            ParameterExpression targetParam = Expression.Parameter(targetType, "target");

            var binaryExpressions = new List<Expression>();
            foreach (var memberPair in memberPairs)
            {
                // Generate code like:
                // target.targetMember = source.sourceMember
                MemberExpression left = memberPair.TargetMember.MemberType == MemberTypes.Property ?
                    Expression.Property(targetParam, targetType.GetProperty(memberPair.TargetMember.Name)) : Expression.Field(targetParam, targetType.GetField(memberPair.TargetMember.Name));

                MemberExpression right = memberPair.SourceMember.MemberType == MemberTypes.Property ?
                    Expression.Property(sourceParam, sourceType.GetProperty(memberPair.SourceMember.Name)) : Expression.Field(sourceParam, sourceType.GetField(memberPair.SourceMember.Name));

                binaryExpressions.Add(Expression.Assign(left, right));
            }
            binaryExpressions.Add(targetParam);

            var block = Expression.Block(binaryExpressions);
            return Expression.Lambda<Func<TSource, TTarget, TTarget>>(block, new[] { sourceParam, targetParam }).Compile();
        }

        /// <summary>
        /// 生成代码：将 TSource 里 同名不同类型的属性 使用IRootMapper.Map转换后赋值给 Target
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TTarget"></typeparam>
        /// <param name="memberPairs"></param>
        /// <returns></returns>
        internal static Func<TSource, TTarget, IRootMapper, TTarget> GenerateSameNameDifferentTypeCopy<TSource, TTarget>(List<SourceTargetMemberPair> memberPairs)
        {
            if (memberPairs.Count == 0)
            {
                return (source, target, rootMapper) => target;
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
            foreach (var memberPair in memberPairs)
            {
                // Generate code like:
                // target.targetMember = (targetMemberType)rootMapper.Map(sourceMemberType, targetMemberType, (object)source.sourceMember, (object)null)
                // target.Point1 = (Point)rootMapper.Map(typeof(Point?), typeof(Point), source.Point1, null)
                MemberExpression left = memberPair.TargetMember.MemberType == MemberTypes.Property ?
                    Expression.Property(targetParam, targetType.GetProperty(memberPair.TargetMember.Name)) : Expression.Field(targetParam, targetType.GetField(memberPair.TargetMember.Name));
                MemberExpression sourceMember = memberPair.SourceMember.MemberType == MemberTypes.Property ?
                    Expression.Property(sourceParam, sourceType.GetProperty(memberPair.SourceMember.Name)) : Expression.Field(sourceParam, sourceType.GetField(memberPair.SourceMember.Name));

                var sourceMemberType = Expression.Constant(memberPair.SourceMember.Type, typeof(Type));
                var targetMemberType = Expression.Constant(memberPair.TargetMember.Type, typeof(Type));
                var sourceObj = Expression.Convert(sourceMember, typeof(object));
                var targetObj = Expression.Constant(null, typeof(object));
                MethodCallExpression callExpression = Expression.Call(rootMapperParam, methodInfo, sourceMemberType, targetMemberType, sourceObj, targetObj);
                // 将 Map() 方法返回的结果强转为 targetMemberType
                var convertExpression = Expression.Convert(callExpression, memberPair.TargetMember.Type);
                binaryExpressions.Add(Expression.Assign(left, convertExpression));
            }
            binaryExpressions.Add(targetParam); // work as "return value;"

            var block = Expression.Block(binaryExpressions);
            return Expression.Lambda<Func<TSource, TTarget, IRootMapper, TTarget>>(block, new[] { sourceParam, targetParam, rootMapperParam }).Compile();
        }
    }
}
