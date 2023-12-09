using System.Reflection;

namespace Fixie.Console;

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