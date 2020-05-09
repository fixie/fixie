namespace Fixie.Cli
{
    using System;
    using System.Reflection;

    class PositionalArgument
    {
        public PositionalArgument(ParameterInfo parameter)
        {
            Type = parameter.ParameterType;
            Name = parameter.Name!;
        }

        public Type Type { get; }
        public string Name { get; }
        public object? Value { get; set; }
    }
}