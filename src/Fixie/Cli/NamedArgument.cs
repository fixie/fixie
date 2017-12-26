namespace Fixie.Cli
{
    using System;
    using System.Collections.Generic;

    class NamedArgument
    {
        public NamedArgument(Type type, string identifier)
        {
            IsArray = type.IsArray;

            ItemType = IsArray
                ? type.GetElementType()
                : type;

            Identifier = identifier;
            Name = Normalize(Identifier);
            Values = new List<object>();
        }

        public bool IsArray { get; }
        public Type ItemType { get; }
        public string Identifier { get; }
        public string Name { get; }
        public List<object> Values { get; }

        public static string Normalize(string namedArgumentKey)
            => namedArgumentKey.ToLower().Replace("-", "");
    }
}