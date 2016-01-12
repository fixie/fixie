using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fixie.Tests
{
    public class InputAttributeParameterSource : ParameterSource
    {
        public IEnumerable<object[]> GetParameters(MethodInfo method)
            => method.GetCustomAttributes<InputAttribute>().Select(x => x.Parameters);
    }
}