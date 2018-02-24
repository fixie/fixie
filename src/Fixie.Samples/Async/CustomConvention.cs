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
            public void Execute(RunContext runContext, Action<CaseAction> runCases)
            {
                runCases(@case =>
                {
                    var instance = Activator.CreateInstance(runContext.TestClass);

                    runContext.Execute(instance, "SetUp");
                    @case.Execute(instance);

                    (instance as IDisposable)?.Dispose();
                });
            }
        }
    }
}