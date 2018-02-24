namespace Fixie.Samples.Async
{
    using System;

    public class CustomConvention : Convention
    {
        public CustomConvention()
        {
            Classes
                .InTheSameNamespaceAs(typeof(CustomConvention))
                .NameEndsWith("Tests");

            Methods
                .Where(method => method.Name != "SetUp")
                .OrderBy(x => x.Name, StringComparer.Ordinal);

            ClassExecution
                .Lifecycle<SetUpLifecycle>();
        }

        class SetUpLifecycle : Lifecycle
        {
            public void Execute(TestClass testClass, Action<CaseAction> runCases)
            {
                runCases(@case =>
                {
                    var instance = Activator.CreateInstance(testClass.Type);

                    testClass.Execute(instance, "SetUp");
                    @case.Execute(instance);

                    (instance as IDisposable)?.Dispose();
                });
            }
        }
    }
}