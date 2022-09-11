using System;
using System.Collections.Generic;
using System.Diagnostics;
using ZK.Mapper.Help;

namespace ZK.Mapper.Core
{
    /// <summary>
    /// Include SourceType, TargetType Info
    /// </summary>
    [DebuggerDisplay("SourceType={SourceType.Name}  TargetType={TargetType.Name}")]
    public class TypePair
    {
        private readonly int hashCode;

        public TypePair(Type sourceType, Type targetType)
        {
            SourceType = sourceType;
            TargetType = targetType;
            hashCode = SourceType.GetHashCode() ^ TargetType.GetHashCode();
        }

        public Type SourceType { get; }

        public Type TargetType { get; }

        public Func<object> TargetParameterlessCtor
        {
            get
            {
                if (targetParameterlessCtors.TryGetValue(this, out var ctor))
                {
                    return ctor;
                }

                if (!TypeHelp.HasParameterlessCtor(TargetType))
                {
                    lock (targetParameterlessCtors)
                    {
                        targetParameterlessCtors[this] = null;
                    }
                    return null;
                }
                var targetParameterlessCtor = ExpressionGenerator.CreateParameterlessCtor(TargetType);
                lock (targetParameterlessCtors)
                {
                    targetParameterlessCtors[this] = targetParameterlessCtor;
                }
                return targetParameterlessCtor;
            }
        }

        public override bool Equals(object obj)
        {
            if (!(obj is TypePair))
            {
                return false;
            }
            var pair = (TypePair)obj;
            return SourceType == pair.SourceType && TargetType == pair.TargetType;
        }

        public override int GetHashCode()
        {
            return hashCode;
        }


        public static TypePair Create<TTSource, TTarget>()
        {
            return new TypePair(typeof(TTSource), typeof(TTarget));
        }

        public static TypePair Create(Type sourceType, Type targetType)
        {
            return new TypePair(sourceType, targetType);
        }

        private static readonly Dictionary<TypePair, Func<object>> targetParameterlessCtors = new Dictionary<TypePair, Func<object>>();

    }
}
