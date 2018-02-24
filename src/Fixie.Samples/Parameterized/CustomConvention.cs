namespace Fixie.Samples.Parameterized
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public class CustomConvention : Convention
    {
        public CustomConvention()
        {
            Methods
                .OrderBy((methodA, methodB) => String.Compare(methodA.Name, methodB.Name, StringComparison.Ordinal));

            Classes
                .InTheSameNamespaceAs(typeof(CustomConvention))
                .NameEndsWith("Tests");

            ClassExecution
                .Lifecycle<CreateInstancePerClass>();

            Parameters
                .Add<InputAttributeParameterSource>();
        }

        class InputAttributeParameterSource : ParameterSource
        {
            public IEnumerable<object[]> GetParameters(MethodInfo method)
            {
                return method.GetCustomAttributes<InputAttribute>(true).Select(input => input.Parameters);
            }
        }
    }
}