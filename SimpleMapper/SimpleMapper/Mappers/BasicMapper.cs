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
        private readonly Func<TSource, TTarget, MapContext, TTarget> sameNameSameTypeCopy;
        private readonly Func<TSource, TTarget, MapContext, IRootMapper, TTarget> sameNameDifferentTypeCopy;

        public BasicMapper(IRootMapper rootMapper)
            : base(new TypePair(typeof(TSource), typeof(TTarget)), rootMapper)
        {
            var sourceMembers = TypePair.SourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty).Where(o => o.CanRead).Select(o => new SourceTargetMemberInfo(o))
                .Concat(TypePair.SourceType.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetField).Select(o => new SourceTargetMemberInfo(o))).ToList();
            var targetMembers = TypePair.TargetType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty).Where(o => o.CanWrite).Select(o => new SourceTargetMemberInfo(o))
                .Concat(TypePair.TargetType.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetField).Select(o => new SourceTargetMemberInfo(o))).ToList();

            var sameNameSameTypes = new List<SourceTargetMemberPair>();
            var sameNameDifferentTypes = new List<SourceTargetMemberPair>();
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
            }
            sameNameSameTypeCopy = ExpressionGenerator.GenerateSameNameSameTypeCopy<TSource, TTarget>(sameNameSameTypes);
            sameNameDifferentTypeCopy = ExpressionGenerator.GenerateSameNameDifferentTypeCopy<TSource, TTarget>(sameNameDifferentTypes);

            SameNameSameTypes = sameNameSameTypes.Select(o => o.SourceMember.Name).ToArray();
            SameNameDifferentTypes = sameNameDifferentTypes.Select(o => o.TargetMember.Name).ToArray();
        }

        public string[] SameNameSameTypes { get; }

        public string[] SameNameDifferentTypes { get; }


        protected override object MapCore(object source, object target, MapContext mapContext)
        {
            if (TypePair.SourceType == TypePair.TargetType)
            {
                return source;
            }
            if (source == null)
            {
                return target ?? default(TTarget);
            }

            if (target == null)
            {
                if (!TypePair.TargetTypeHasParameterlessCtor)
                {
                    return target ?? default(TTarget);
                }
                target = TypePair.TargetParameterlessCtor();
            }

            return MapCoreGeneric((TSource)source, (TTarget)target, mapContext);
        }

        private TTarget MapCoreGeneric(TSource source, TTarget target, MapContext mapContext)
        {
            target = sameNameSameTypeCopy(source, target, mapContext);
            target = sameNameDifferentTypeCopy(source, target, mapContext, RootMapper);
            return target;
        }

         

    }


    internal class ClassMapperBuilder : MapperBuilderBase
    {
        public ClassMapperBuilder(IRootMapper rootMapper) : base(rootMapper)
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
