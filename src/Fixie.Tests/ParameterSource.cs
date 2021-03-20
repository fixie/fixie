namespace Fixie.Tests
{
    using System.Collections.Generic;
    using System.Reflection;

    public delegate IEnumerable<object?[]> ParameterSource(MethodInfo method);
}