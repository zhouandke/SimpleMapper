using System;
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
        private Func<object> targetParameterlessCtor;

        public TypePair(Type sourceType, Type targetType)
        {
            SourceType = sourceType;
            TargetType = targetType;
            TargetTypeHasParameterlessCtor = TypeHelp.HasParameterlessCtor(TargetType);
            hashCode = SourceType.GetHashCode() ^ TargetType.GetHashCode();
        }

        public Type SourceType { get; }

        public Type TargetType { get; }

        public bool TargetTypeHasParameterlessCtor { get; }

        public Func<object> TargetParameterlessCtor
        {
            get
            {
                if (!TargetTypeHasParameterlessCtor)
                {
                    return null;
                }
                if (targetParameterlessCtor == null)
                {
                    targetParameterlessCtor = ExpressionGenerator.CreateParameterlessCtor(TargetType);
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
    }
}
