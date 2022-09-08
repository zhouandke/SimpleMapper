using System;
using ZK.Mapper.Help;

namespace ZK.Mapper.Core
{
    public class TypePair
    {
        private readonly int hashCode;
        private Func<object> targetEmptyCtor;

        public TypePair(Type sourceType, Type targetType)
        {
            SourceType = sourceType;
            TargetType = targetType;
            TargetTypeHasEmptyCtor = TypeHelp.HasDefaultCtor(TargetType);
            hashCode = SourceType.GetHashCode() ^ TargetType.GetHashCode();
        }

        public Type SourceType { get; }

        public Type TargetType { get; }

        public bool TargetTypeHasEmptyCtor { get; }

        public Func<object> TargetEmptyCtor
        {
            get
            {
                if (!TargetTypeHasEmptyCtor)
                {
                    return null;
                }
                if (targetEmptyCtor == null)
                {
                    targetEmptyCtor = ExpressionGenerator.CreateEmptyCtor(TargetType);
                }
                return targetEmptyCtor;
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
