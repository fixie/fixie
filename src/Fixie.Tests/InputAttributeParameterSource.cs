namespace Fixie.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public class InputAttributeParameterSource : ParameterSource
    {
        public IEnumerable<object?[]> GetParameters(MethodInfo method)
        {
            var inputAttributes = method.GetCustomAttributes<InputAttribute>(true)
                .OrderBy(x => x.Order)
                .ToArray();

            if (inputAttributes.Any())
                foreach (var input in inputAttributes)
                    yield return input.Parameters;
        }
    }
}