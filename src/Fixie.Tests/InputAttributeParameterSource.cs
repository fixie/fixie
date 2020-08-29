namespace Fixie.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public class InputAttributeParameterSource : ParameterSource
    {
        public IEnumerable<object?[]> GetParameters(MethodInfo method)
            => method
                .GetCustomAttributes<InputAttribute>(true)
                .Select(input => input.Parameters);
    }
}