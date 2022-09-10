using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Reflection;

namespace ZK.Mapper.Help
{
    internal class ExpressionTreeSample
    {
        static void Test(string[] args)
        {
            TestIEnumerableToList();
            TestIEnumerableToArray();
        }

        static void TestIEnumerableToArray()
        {
            var sourceType = typeof(int);
            var targetType = typeof(string);

            var sourceParam = Expression.Parameter(typeof(object), "sourceObj");
            var targetParam = Expression.Parameter(typeof(object), "targetObj");
            var convertParam = Expression.Parameter(typeof(object), "convertObj");
            Expression<Func<int, string>> convert = intValue => intValue.ToString();

            var call = Expression.Call(typeof(MethodClass), nameof(MethodClass.IEnumerableToArray), new Type[] { sourceType, targetType }, sourceParam, targetParam, convert);
            var func = Expression.Lambda<Func<object, object, object, object>>(call, sourceParam, targetParam, convertParam).Compile();

            var src = new[] { 1, 2, 3 };
            var target = new string[8];
            var l = func(src, target, convert);
        }

        static void TestIEnumerableToList()
        {
            var sourceType = typeof(int);
            var targetType = typeof(string);

            var sourceParam = Expression.Parameter(typeof(IEnumerable<>).MakeGenericType(sourceType), "sources");
            var targetParam = Expression.Parameter(typeof(List<>).MakeGenericType(targetType), "targets");
            var convertParam = Expression.Parameter(typeof(Func<,>).MakeGenericType(sourceType, targetType), "convertObj");

            Expression<Func<int, string>> convert = intValue => intValue.ToString();

            var call = Expression.Call(typeof(MethodClass), nameof(MethodClass.IEnumerableToList), new Type[] { typeof(int), typeof(string) }, sourceParam, targetParam, convert);
            // 动态创建 Func
            var lambdaType = typeof(Func<,,,>).MakeGenericType(
                typeof(IEnumerable<>).MakeGenericType(sourceType),
                typeof(List<>).MakeGenericType(targetType),
                typeof(Func<,>).MakeGenericType(sourceType, targetType),
                typeof(List<>).MakeGenericType(targetType));

            var func = Expression.Lambda(lambdaType, call, sourceParam, targetParam, convertParam).Compile();
            var src = new[] { 1, 2, 3 };
            var target = func.DynamicInvoke(src, null, convert.Compile());
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



        // 一个比较完整的例子
        private static void ExpressionTreeAccessCollection(object input)
        {
            var type = input.GetType();
            var inputParameterExp = Expression.Parameter(typeof(object), "input");
            var callbackParameterExp = Expression.Parameter(typeof(Action<object>), "callback");
            var countVariableExp = Expression.Variable(typeof(int), "count");
            var tempVariableExp = Expression.Variable(typeof(int));
            var itemVariableExp = Expression.Variable(typeof(object), "item");
            var convertExp = Expression.Convert(inputParameterExp, type);
            var voidLabel = Expression.Label();

            // 方法一
            var indexProperty = type.GetDefaultMembers().OfType<PropertyInfo>()
                .First(_ => _.GetIndexParameters().Any(o => o.ParameterType == typeof(int)));

            // 方法二
            //var toArrayMethod = type.GetMethod(nameof(List<object>.ToArray));
            //var toArrayExp = Expression.Call(convertExp, toArrayMethod);
            //var arrayIndexExp = Expression.ArrayIndex(toArrayExp, new Expression[] { tempVariableExp });

            // 调用外部方法
            //var printItemMethod = typeof(Program).GetMethod(nameof(PrintItem), BindingFlags.NonPublic | BindingFlags.Static);

            var blockExp = Expression.Block(
                new ParameterExpression[] { countVariableExp, tempVariableExp, itemVariableExp },
                Expression.Assign(
                    countVariableExp,
                    Expression.Property(convertExp, "Count")
                ),
                Expression.Assign(tempVariableExp, Expression.Constant(0)),
                Expression.Loop(
                    Expression.IfThenElse(
                        Expression.LessThan(tempVariableExp, countVariableExp),
                        Expression.Block(
                            new ParameterExpression[] { itemVariableExp },
                            // 方法一
                            Expression.Assign(itemVariableExp, Expression.MakeIndex(convertExp, indexProperty, new ParameterExpression[] { tempVariableExp })),

                            // 方法二
                            //Expression.Assign(itemVariableExp, arrayIndexExp),

                            // 调用外部方法
                            //Expression.Call(null, printItemMethod, itemVariableExp),

                            // 调用回调函数
                            Expression.Invoke(callbackParameterExp, itemVariableExp),
                            Expression.AddAssign(tempVariableExp, Expression.Constant(1, typeof(int)))
                        ),
                        Expression.Block(
                            Expression.Return(voidLabel)
                        )
                    )
                ),
                Expression.Label(voidLabel)
            );

            var lambda = Expression.Lambda<Action<object, Action<object>>>(blockExp, new ParameterExpression[] { inputParameterExp, callbackParameterExp });
            var func = lambda.Compile();

            //func(input, item => {
            //    Console.WriteLine($"Callback: {JsonSerializer.Serialize(item)}");
            //});
        }
    }

    static class MethodClass
    {
        public static object IEnumerableToArray<TSource, TTarget>(object sourceObj, object targetObj, object convertObj)
        {
            if (sourceObj == null)
            {
                return null;
            }

            var convert = (Func<TSource, TTarget>)convertObj;
            var sources = (IEnumerable<TSource>)sourceObj;
            var count = sources.Count();
            TTarget[] targets = targetObj != null ? (TTarget[])targetObj : new TTarget[count];
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
                targets[index] = convert(source);
                index++;
            }
            return targets;
        }


        public static List<TTarget> IEnumerableToList<TSource, TTarget>(IEnumerable<TSource> sources, List<TTarget> targets, Func<TSource, TTarget> convert)
        {
            if (sources == null)
            {
                return null;
            }

            if (targets != null)
            {
                targets.Clear();
            }
            else
            {
                targets = new List<TTarget>();
            }

            foreach (var source in sources)
            {
                targets.Add(convert(source));
            }

            return targets;
        }
    }

    public class CallThis
    {
        public void Call()
        {
            MethodInfo methodInfo = typeof(CallThis).GetMethod("Show", new Type[] { typeof(int), typeof(int) });
            ParameterExpression paramA = Expression.Parameter(typeof(int), "a");
            ParameterExpression paramB = Expression.Parameter(typeof(int), "b");
            var thisParameter = Expression.Constant(this);
            MethodCallExpression methodCall = Expression.Call(thisParameter, methodInfo, paramA, paramB);
            var lambda = Expression.Lambda<Action<int, int>>(methodCall, new ParameterExpression[] { paramA, paramB });
            Action<int, int> func = lambda.Compile();

            func(1, 2);
        }

        public void Show(int a, int b)
        {
            Console.WriteLine(a + b);
        }
    }
}
