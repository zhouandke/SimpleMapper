using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using ZK.Mapper.Core;
using ZK.Mapper.Help;

namespace ZK.Mapper.Mappers
{
    /// <summary>
    /// 同名同类型的属性直接拷贝，同名不同类型的属性使用IRootMapper.Map转换后赋值给 Target
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TTarget"></typeparam>
    internal class BasicMapper<TSource, TTarget> : MapperBase
    {
        #region ShallowCopy
        private readonly Func<TSource, TTarget, MapContext, TTarget> sameNameSameTypeAssign;
        private readonly Func<TSource, TTarget, MapContext, IRootMapper, TTarget> sameNameDifferentTypeAssign;
        public readonly string[] SameNameSameTypeMembers;
        public readonly string[] SameNameDifferentTypeMembers;
        #endregion

        #region DeepCopy
        private readonly Func<TSource, TTarget, MapContext, TTarget> directAssign;
        private readonly Func<TSource, TTarget, MapContext, IRootMapper, TTarget> mapThenAssign;
        public readonly string[] DirectAssignMembers;
        public readonly string[] MapThenAssignMembers;
        #endregion

        public BasicMapper(IRootMapper rootMapper)
            : base(new TypePair(typeof(TSource), typeof(TTarget)), rootMapper)
        {
            var sourceMembers = TypePair.SourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty).Where(o => o.CanRead).Select(o => new SourceTargetMemberInfo(o))
                .Concat(TypePair.SourceType.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetField).Select(o => new SourceTargetMemberInfo(o))).ToList();
            var targetMembers = TypePair.TargetType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty).Where(o => o.CanWrite).Select(o => new SourceTargetMemberInfo(o))
                .Concat(TypePair.TargetType.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetField).Select(o => new SourceTargetMemberInfo(o))).ToList();

            var sameNameSameTypes = new List<SourceTargetMemberPair>();
            var sameNameDifferentTypes = new List<SourceTargetMemberPair>();
            var directAssignMembers = new List<SourceTargetMemberPair>();
            var mapThenAssignMembers = new List<SourceTargetMemberPair>(); ;
            foreach (var sourceMember in sourceMembers)
            {
                var targetMember = targetMembers.Find(o => o.Name == sourceMember.Name);
                if (targetMember == null)
                {
                    continue;
                }

                if (targetMember.Type == sourceMember.Type)
                {
                    sameNameSameTypes.Add(new SourceTargetMemberPair(sourceMember, targetMember));
                }
                else
                {
                    sameNameDifferentTypes.Add(new SourceTargetMemberPair(sourceMember, targetMember));
                }

                if (targetMember.Type == sourceMember.Type && RootMapper.ImmutableTypeManage.IsImmutable(targetMember.Type))
                {
                    directAssignMembers.Add(new SourceTargetMemberPair(sourceMember, targetMember));
                }
                else
                {
                    mapThenAssignMembers.Add(new SourceTargetMemberPair(sourceMember, targetMember));
                }
            }
            sameNameSameTypeAssign = ExpressionGenerator.GenerateDirectAssign<TSource, TTarget>(sameNameSameTypes);
            sameNameDifferentTypeAssign = ExpressionGenerator.GenerateMapThenAssign<TSource, TTarget>(sameNameDifferentTypes);

            directAssign = ExpressionGenerator.GenerateDirectAssign<TSource, TTarget>(directAssignMembers);
            mapThenAssign = ExpressionGenerator.GenerateMapThenAssign<TSource, TTarget>(mapThenAssignMembers);

            SameNameSameTypeMembers = sameNameSameTypes.Select(o => o.SourceMember.Name).ToArray();
            SameNameDifferentTypeMembers = sameNameDifferentTypes.Select(o => o.TargetMember.Name).ToArray();
            DirectAssignMembers = directAssignMembers.Select(o => o.SourceMember.Name).ToArray();
            MapThenAssignMembers = mapThenAssignMembers.Select(o => o.SourceMember.Name).ToArray();
        }

        protected override object MapCore(object source, object target, MapContext mapContext)
        {
            if (source == null)
            {
                return target ?? default(TTarget);
            }

            return mapContext.DeepCopy ? DeepCopy(source, target, mapContext) : ShallowCopy(source, target, mapContext);
        }

        protected object ShallowCopy(object source, object target, MapContext mapContext)
        {
            if (TypePair.SourceType == TypePair.TargetType)
            {
                return source;
            }

            if (target == null)
            {
                if (TypePair.TargetParameterlessCtor == null)
                {
                    return target ?? default(TTarget);
                }
                target = TypePair.TargetParameterlessCtor();
            }

            return ShallowCopyGeneric((TSource)source, (TTarget)target, mapContext);
        }

        private TTarget ShallowCopyGeneric(TSource source, TTarget target, MapContext mapContext)
        {
            target = sameNameSameTypeAssign(source, target, mapContext);
            target = sameNameDifferentTypeAssign(source, target, mapContext, RootMapper);
            return target;
        }

        protected object DeepCopy(object source, object target, MapContext mapContext)
        {
            if (TypePair.SourceType == TypePair.TargetType && RootMapper.ImmutableTypeManage.IsImmutable(TypePair.SourceType))
            {
                return source;
            }

            if (TypePair.TargetParameterlessCtor == null)
            {
                return target ?? default(TTarget);
            }
            target = TypePair.TargetParameterlessCtor();

            return DeepCopyGeneric((TSource)source, (TTarget)target, mapContext);
        }

        protected TTarget DeepCopyGeneric(TSource source, TTarget target, MapContext mapContext)
        {
            target = directAssign(source, target, mapContext);
            target = mapThenAssign(source, target, mapContext, RootMapper);
            return target;
        }
    }


    internal class BasicMapperBuilder : MapperBuilderBase
    {
        public BasicMapperBuilder(IRootMapper rootMapper) : base(rootMapper)
        {
        }

        public override int Priority => 0;

        public override MapperBase Build(TypePair typePair)
        {
            var type = typeof(BasicMapper<,>).MakeGenericType(typePair.SourceType, typePair.TargetType);
            var mapper = (MapperBase)type.GetConstructor(ImplementMapperConstructorTypes).Invoke(new object[] { RootMapper });
            return mapper;
        }
    }
}
