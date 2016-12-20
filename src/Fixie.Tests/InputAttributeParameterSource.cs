namespace Fixie.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public class InputAttributeParameterSource : ParameterSource
    {
        public IEnumerable<object[]> GetParameters(MethodInfo method)
            => method.GetCustomAttributes<InputAttribute>().Select(x => x.Parameters);
    }
}