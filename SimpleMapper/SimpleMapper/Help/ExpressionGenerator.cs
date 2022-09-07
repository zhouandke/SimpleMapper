using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace ZK
{
    internal static class ExpressionGenerator
    {
        public static Func<object> CreateEmptyCtor(Type type)
        {
            return Expression.Lambda<Func<object>>(Expression.New(type)).Compile();
        }

        public static Action<TSource, TTarget> GenerateSameNameSameTypeCopy<TSource, TTarget>(List<Tuple<PropertyFieldInfo, PropertyFieldInfo>> memberTuples)
        {
            Type sourceType = typeof(TSource);
            Type targetType = typeof(TTarget);

            ParameterExpression objSrc = Expression.Parameter(sourceType, "source");
            ParameterExpression objDst = Expression.Parameter(targetType, "target");

            var binaryExpressions = new List<Expression>();
            foreach (var memberTuple in memberTuples)
            {
                MemberExpression left = (memberTuple.Item2.MemberType == MemberTypes.Property) ?
                    Expression.Property(objDst, targetType.GetProperty(memberTuple.Item2.Name)) : Expression.Field(objDst, targetType.GetField(memberTuple.Item2.Name));

                MemberExpression right = (memberTuple.Item1.MemberType == MemberTypes.Property) ?
                    Expression.Property(objSrc, sourceType.GetProperty(memberTuple.Item1.Name)) : Expression.Field(objSrc, sourceType.GetField(memberTuple.Item1.Name));

                binaryExpressions.Add(Expression.Assign(left, right)); // 赋值
            }
            if (binaryExpressions.Count == 0)
            {
                return (target, source) => { };
            }
            var block = Expression.Block(binaryExpressions);
            return Expression.Lambda<Action<TSource, TTarget>>(block, new[] { objSrc, objDst }).Compile();
        }


        internal static Action<TSource, TTarget, IRootMapper> GenerateSameNameDifferentTypeCopy<TSource, TTarget>(List<Tuple<PropertyFieldInfo, PropertyFieldInfo>> memberTuples)
        {
            Type sourceType = typeof(TSource);
            Type targetType = typeof(TTarget);
            Type rootMapperType = typeof(IRootMapper);

            ParameterExpression sourceParam = Expression.Parameter(sourceType, "source");
            ParameterExpression targetParam = Expression.Parameter(targetType, "target");
            ParameterExpression rootMapperParam = Expression.Parameter(rootMapperType, "rootMapper");

            MethodInfo methodInfo = typeof(IRootMapper).GetMethod("Map", new Type[] { typeof(Type), typeof(Type), typeof(object), typeof(object) });

            var binaryExpressions = new List<Expression>();
            foreach (var memberTuple in memberTuples)
            {
                MemberExpression left = (memberTuple.Item2.MemberType == MemberTypes.Property) ?
                    Expression.Property(targetParam, targetType.GetProperty(memberTuple.Item2.Name)) : Expression.Field(targetParam, targetType.GetField(memberTuple.Item2.Name));
                MemberExpression sourceMember = (memberTuple.Item1.MemberType == MemberTypes.Property) ?
                    Expression.Property(sourceParam, sourceType.GetProperty(memberTuple.Item1.Name)) : Expression.Field(sourceParam, sourceType.GetField(memberTuple.Item1.Name));

                var sourceMemberType = Expression.Constant(memberTuple.Item1.Type, typeof(Type));
                var targetMemberType = Expression.Constant(memberTuple.Item2.Type, typeof(Type));
                var sourceObj = Expression.Convert(sourceMember, typeof(object));
                var targetObj = Expression.Constant(null, typeof(object));
                MethodCallExpression callExpression = Expression.Call(rootMapperParam, methodInfo, sourceMemberType, targetMemberType, sourceObj, targetObj);
                var convertExpression = Expression.Convert(callExpression, memberTuple.Item2.Type);
                binaryExpressions.Add(Expression.Assign(left, convertExpression)); // 赋值
            }
            if (binaryExpressions.Count == 0)
            {
                return (target, source, rootMapper) => { };
            }
            var block = Expression.Block(binaryExpressions);
            return Expression.Lambda<Action<TSource, TTarget, IRootMapper>>(block, new[] { sourceParam, targetParam, rootMapperParam }).Compile();
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
