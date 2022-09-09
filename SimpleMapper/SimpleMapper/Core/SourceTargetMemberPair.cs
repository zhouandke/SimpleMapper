namespace ZK.Mapper.Core
{
    internal class SourceTargetMemberPair
    {
        public SourceTargetMemberPair(SourceTargetMemberInfo sourceMember, SourceTargetMemberInfo targetMember)
        {
            SourceMember = sourceMember;
            TargetMember = targetMember;
        }

        public SourceTargetMemberInfo SourceMember { get; }

        public SourceTargetMemberInfo TargetMember { get; }
    }
}
