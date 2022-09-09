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
        /// 编译生成 无参构造器的创建方法, 直接 new 耗时1, 改方法是 2, Activator.CreateInstance 是 3, 反射是70
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Func<object> CreateEmptyCtor(Type type)
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
        /// <param name="memberTuples"></param>
        /// <returns></returns>
        public static Func<TSource, TTarget, TTarget> GenerateSameNameSameTypeCopy<TSource, TTarget>(List<Tuple<PropertyFieldInfo, PropertyFieldInfo>> memberTuples)
        {
            if (memberTuples.Count == 0)
            {
                return (source, target) => target;
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
            binaryExpressions.Add(objDst);

            var block = Expression.Block(binaryExpressions);
            return Expression.Lambda<Func<TSource, TTarget, TTarget>>(block, new[] { objSrc, objDst }).Compile();
        }

        /// <summary>
        /// 生成代码：将 TSource 里 同名不同类型的属性 使用IRootMapper.Map转换后赋值给 Target
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TTarget"></typeparam>
        /// <param name="memberTuples"></param>
        /// <returns></returns>
        internal static Func<TSource, TTarget, IRootMapper, TTarget> GenerateSameNameDifferentTypeCopy<TSource, TTarget>(List<Tuple<PropertyFieldInfo, PropertyFieldInfo>> memberTuples)
        {
            if (memberTuples.Count == 0)
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
            MethodInfo methodInfo = typeof(IRootMapper).GetMethod("Map");

            var binaryExpressions = new List<Expression>();
            var _Variable = Expression.Variable(typeof(object), "_");
            var _VariableAssign = Expression.Assign(_Variable, Expression.Constant(null, typeof(object)));
            //binaryExpressions.Add(_VariableAssign);
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
                //var targetObj = Expression.Constant(null, typeof(object));
                binaryExpressions.Add(_VariableAssign);
                MethodCallExpression callExpression = Expression.Call(rootMapperParam, methodInfo, sourceMemberType, targetMemberType, sourceObj, _Variable);
                // 将 Map() 方法返回的结果强转为 targetMemberType
                var convertExpression = Expression.Convert(callExpression, memberTuple.Item2.Type);
                binaryExpressions.Add(Expression.Assign(left, convertExpression));
            }
            binaryExpressions.Add(targetParam);

            var block = Expression.Block(new[] { _Variable }, binaryExpressions);
            return Expression.Lambda<Func<TSource, TTarget, IRootMapper, TTarget>>(block, new[] { sourceParam, targetParam, rootMapperParam }).Compile();
        }
    }
}
