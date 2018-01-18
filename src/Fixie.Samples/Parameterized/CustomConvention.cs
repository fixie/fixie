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
            Classes
                .InTheSameNamespaceAs(typeof(CustomConvention))
                .NameEndsWith("Tests");

            Methods
                .Where(method => method.IsVoid());

            ClassExecution
                .Lifecycle<CreateInstancePerClass>()
                .SortCases((caseA, caseB) => String.Compare(caseA.Name, caseB.Name, StringComparison.Ordinal));

            Parameters
                .Add<InputAttributeParameterSource>();
        }

        class CreateInstancePerClass : Lifecycle
        {
            public void Execute(Type testClass, Action<CaseAction> runCases)
            {
                var instance = Activator.CreateInstance(testClass);

                runCases(@case => @case.Execute(instance));

                (instance as IDisposable)?.Dispose();
            }
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