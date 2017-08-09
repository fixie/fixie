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

        public Array CreateTypedValuesArray()
        {
            Array destinationArray = Array.CreateInstance((Type) ItemType, (int) Values.Count);
            Array.Copy(Values.ToArray(), destinationArray, Values.Count);
            return destinationArray;
        }

        public static string Normalize(string namedArgumentKey)
            => namedArgumentKey.ToLower().Replace("-", "");
    }
}