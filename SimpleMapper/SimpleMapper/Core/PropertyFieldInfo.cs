using System;
using System.Reflection;

namespace ZK.Mapper.Core
{
    internal class PropertyFieldInfo
    {
        public PropertyFieldInfo(PropertyInfo propertyInfo)
        {
            PropertyInfo = propertyInfo;
            MemberType = MemberTypes.Property;
            Name = propertyInfo.Name;
            Type = propertyInfo.PropertyType;
        }

        public PropertyFieldInfo(FieldInfo fieldInfo)
        {
            FieldInfo = fieldInfo;
            MemberType = MemberTypes.Field;
            Name = fieldInfo.Name;
            Type = fieldInfo.FieldType;
        }

        public PropertyInfo PropertyInfo { get; }

        public FieldInfo FieldInfo { get; }

        public MemberTypes MemberType { get; }

        public string Name { get; }

        public Type Type { get; }
    }
}
