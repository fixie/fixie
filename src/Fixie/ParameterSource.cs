using System.Collections.Generic;
using System.Reflection;

namespace Fixie
{
    public interface ParameterSource
    {
        IEnumerable<object[]> GetParameters(MethodInfo method);
    }
}