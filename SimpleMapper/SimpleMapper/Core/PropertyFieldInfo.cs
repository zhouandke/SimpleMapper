using System;
using System.Reflection;

namespace ZK
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

        public PropertyInfo PropertyInfo { get; set; }

        public FieldInfo FieldInfo { get; set; }

        public MemberTypes MemberType { get; set; }

        public string Name { get; set; }

        public Type Type { get; set; }
    }
}
