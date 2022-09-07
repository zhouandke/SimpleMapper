﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ZK
{
    public class ClassMapper<TSource, TTarget> : MapperBase
    {
        private Action<TSource, TTarget> sameNameSameTypeCopy;
        private Action<TSource, TTarget, IRootMapper> sameNameDifferentTypeCopy;

        public ClassMapper(IRootMapper rootMapper)
            : base(new TypePair(typeof(TSource), typeof(TTarget)), rootMapper)
        {
            var sourceMembers = (TypePair.SourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty).Select(o => new PropertyFieldInfo(o)))
                .Concat(TypePair.SourceType.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetField).Select(o => new PropertyFieldInfo(o))).ToList();
            var targetMembers = (TypePair.TargetType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty).Select(o => new PropertyFieldInfo(o)))
                .Concat(TypePair.TargetType.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetField).Select(o => new PropertyFieldInfo(o))).ToList();

            var sameNameSameTypes = new List<Tuple<PropertyFieldInfo, PropertyFieldInfo>>();
            var sameNameDifferentTypes = new List<Tuple<PropertyFieldInfo, PropertyFieldInfo>>();
            foreach (var sourceMember in sourceMembers)
            {
                var targetMember = targetMembers.Find(o => o.Name == sourceMember.Name);
                if (targetMember == null)
                {
                    continue;
                }
                if (targetMember.Type == sourceMember.Type)
                {
                    sameNameSameTypes.Add(Tuple.Create(sourceMember, targetMember));
                }
                else
                {
                    sameNameDifferentTypes.Add(Tuple.Create(sourceMember, targetMember));
                }
            }
            sameNameSameTypeCopy = ExpressionGenerator.GenerateSameNameSameTypeCopy<TSource, TTarget>(sameNameSameTypes);
            sameNameDifferentTypeCopy = ExpressionGenerator.GenerateSameNameDifferentTypeCopy<TSource, TTarget>(sameNameDifferentTypes);

            SameNameSameTypes = sameNameSameTypes.Select(o => o.Item1.Name).ToArray();
            SameNameDifferentTypes = sameNameDifferentTypes.Select(o => o.Item1.Name).ToArray();
        }

        public string[] SameNameSameTypes { get; }

        public string[] SameNameDifferentTypes { get; }




        //void SameNameDifferentTypeCopy(TSource source, TTarget target, IRootMapper rootMapper)
        //{
        //    target.kk = rootMapper.Map(sourceMemberType, targetMemberType, source.kk);
        //}


        protected override object MapCore(object source, object target)
        {
            if (TypePair.SourceType == TypePair.TargetType)
            {
                return source;
            }
            if (true)
            {

            }
            return MapCore((TSource)source, (TTarget)target);
        }

        private TTarget MapCore(TSource source, TTarget target)
        {
            if (target == null)
            {
                if (!TypePair.TargetTypeHasEmptyCtor)
                {
                    return target;
                }
                target = (TTarget)TypePair.TargetEmptyCtor();
            }
            sameNameSameTypeCopy(source, target);
            sameNameDifferentTypeCopy(source, target, RootMapper);
            return target;
        }

        public override int GetHashCode()
        {
            return TypePair.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var another = obj as ClassMapper<TSource, TTarget>;
            if (another == null)
            {
                return false;
            }
            return TypePair.Equals(another.TypePair);
        }
    }



}