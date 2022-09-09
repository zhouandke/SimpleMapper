using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.ObjectModel;

namespace ZK.Mapper.Help
{
    internal static class TypeHelp
    {
        internal static bool IsPrimitiveNullable(Type type, out Type primitiveType)
        {
            if (type.IsPrimitive || type == typeof(decimal) || type.IsEnum)
            {
                primitiveType = type;
                return true;
            }
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                primitiveType = type.GetGenericArguments()[0];
                if (primitiveType.IsPrimitive || primitiveType == typeof(decimal) || type.IsEnum)
                {
                    return true;
                }
            }
            primitiveType = null;
            return false;
        }

        public static bool IsNullable(Type type, out Type underlyingType)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                underlyingType = type.GetGenericArguments()[0];
                return true;
            }
            underlyingType = type;
            return false;
        }

        public static bool IsEnum(Type type, out Type enumType)
        {
            if (type.IsEnum)
            {
                enumType = type;
                return true;
            }
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                enumType = type.GetGenericArguments()[0];
                if (enumType.IsEnum)
                {
                    return true;
                }
            }

            enumType = null;
            return false;
        }



        // 
        private static readonly Type[] numberForEnumMapSupportTypes = new Type[]
        {
            typeof(bool),  // Maybe one enum type contain True, False
            typeof(byte),
            typeof(sbyte),
            typeof(char),  // I think it is confused that char convert to enum, but I support it
            typeof(short),
            typeof(ushort),
            typeof(int),
            typeof(uint),
            typeof(long),
            typeof(ulong),
            typeof(float),
            typeof(double),
            typeof(decimal),
        };
        public static bool IsNumberForEnumMap(Type type, out Type numberType)
        {
            if (numberForEnumMapSupportTypes.Contains(type))
            {
                numberType = type;
                return true;
            }
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                numberType = type.GetGenericArguments()[0];
                if (numberForEnumMapSupportTypes.Contains(numberType))
                {
                    return true;
                }
            }

            numberType = null;
            return false;
        }

        public static bool IsDictionaryOf(Type type, out Type keyType, out Type valueType)
        {
            if (IsGenericType(type) &&
                   (type.GetGenericTypeDefinition() == typeof(Dictionary<,>) ||
                    type.GetGenericTypeDefinition() == typeof(IDictionary<,>)))
            {
                var genericArguments = type.GetGenericArguments();
                keyType = genericArguments[0];
                valueType = genericArguments[1];
                return true;
            }
            else
            {
                keyType = null;
                valueType = null;
                return false;
            }
        }

        public static Type GetEnumerableItemType(Type type)
        {
            if (type.IsArray)
            {
                return type.GetElementType();
            }
            if (IsGenericType(type) && IsIEnumerableOf(type))
            {
                return type.GetGenericArguments().First();
            }

            return typeof(object);
        }

        public static ConstructorInfo GetDefaultCtor(Type type)
        {
            return type.GetConstructor(Type.EmptyTypes);
        }

        public static KeyValuePair<Type, Type> GetDictionaryItemTypes(Type type)
        {
            if (IsDictionaryOf(type))
            {
                Type[] types = type.GetGenericArguments();
                return new KeyValuePair<Type, Type>(types[0], types[1]);
            }
            throw new NotSupportedException();
        }

        public static MethodInfo GetGenericMethod(Type type, string methodName, params Type[] arguments)
        {
            return type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)
                       .MakeGenericMethod(arguments);
        }

        public static bool HasParameterlessCtor(Type type)
        {
            return type.GetConstructor(Type.EmptyTypes) != null
                || type.IsValueType; // 值类型 GetConstructor(Type.EmptyTypes) 始终是 null, 但值类型绝对有 无参构造函数
        }

        public static bool IsDictionaryOf(Type type)
        {
            return IsGenericType(type) &&
                   (type.GetGenericTypeDefinition() == typeof(Dictionary<,>) ||
                    type.GetGenericTypeDefinition() == typeof(IDictionary<,>));
        }

        public static bool IsIEnumerable(Type type)
        {
            return typeof(IEnumerable).IsAssignableFrom(type);
        }

        public static bool IsIEnumerableOf(Type type)
        {
            return type.GetInterfaces()
                       .Any(x => IsGenericType(x) &&
                                 x.GetGenericTypeDefinition() == typeof(IEnumerable<>) ||
                                 !IsGenericType(x) && x == typeof(IEnumerable));
        }

        public static bool IsArrayOf(Type type)
        {
            return type.IsArray;
        }

        public static bool IsListOf(Type type)
        {
            return IsGenericType(type) && (type.GetGenericTypeDefinition() == typeof(List<>) || type.GetGenericTypeDefinition() == typeof(IList<>));
        }

        public static bool IsCollectionOf(Type type)
        {
            return IsGenericType(type) && (type.GetGenericTypeDefinition() == typeof(Collection<>) || type.GetGenericTypeDefinition() == typeof(ICollection<>));
        }

        public static bool IsHashsetOf(Type type)
        {
            return IsGenericType(type) && type.GetGenericTypeDefinition() == typeof(HashSet<>);
        }



        public static bool IsNullable(Type type)
        {
            return IsGenericType(type) && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        internal static bool IsValueType(Type type)
        {
#if COREFX
            return type.GetTypeInfo().IsValueType;
#else
            return type.IsValueType;
#endif
        }

        internal static bool IsPrimitive(Type type)
        {
#if COREFX
            return type.GetTypeInfo().IsPrimitive;
#else
            return type.IsPrimitive;
#endif
        }

        internal static bool IsEnum(Type type)
        {
#if COREFX
            return type.GetTypeInfo().IsEnum;
#else
            return type.IsEnum;
#endif
        }

        internal static bool IsGenericType(Type type)
        {
#if COREFX
            return type.GetTypeInfo().IsGenericType;
#else
            return type.IsGenericType;
#endif
        }

        internal static Type BaseType(Type type)
        {
#if COREFX
            return type.GetTypeInfo().BaseType;
#else
            return type.BaseType;
#endif
        }
    }
}
