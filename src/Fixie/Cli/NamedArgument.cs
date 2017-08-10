namespace Fixie.Cli
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    class NamedArgument
    {
        public NamedArgument(PropertyInfo property)
        {
            IsArray = property.PropertyType.IsArray;

            ItemType = IsArray
                ? property.PropertyType.GetElementType()
                : property.PropertyType;

            PropertyName = property.Name;
            Name = Normalize(PropertyName);
            Values = new List<object>();
        }

        public bool IsArray { get; }
        public Type ItemType { get; }
        public string PropertyName { get; }
        public string Name { get; }
        public List<object> Values { get; }

        public static string Normalize(string namedArgumentKey)
            => namedArgumentKey.ToLower().Replace("-", "");
    }
}