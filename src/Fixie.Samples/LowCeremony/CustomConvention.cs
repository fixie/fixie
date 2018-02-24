namespace Fixie.Samples.LowCeremony
{
    using System;
    using System.Linq;

    public class CustomConvention : Convention
    {
        static readonly string[] LifecycleMethods = { "FixtureSetUp", "FixtureTearDown", "SetUp", "TearDown" };

        public CustomConvention()
        {
            Classes
                .InTheSameNamespaceAs(typeof(CustomConvention))
                .NameEndsWith("Tests");

            Methods
                .Where(method => LifecycleMethods.All(x => x != method.Name))
                .OrderBy(x => x.Name, StringComparer.Ordinal);

            ClassExecution
                .Lifecycle<CallSetUpTearDownMethodsByName>();
        }

        class CallSetUpTearDownMethodsByName : Lifecycle
        {
            public void Execute(RunContext runContext, Action<CaseAction> runCases)
            {
                var instance = Activator.CreateInstance(runContext.TestClass);

                void Execute(string method)
                    => runContext.Execute(instance, method);

                Execute("FixtureSetUp");
                runCases(@case =>
                {
                    Execute("SetUp");
                    @case.Execute(instance);
                    Execute("TearDown");
                });
                Execute("FixtureTearDown");

                (instance as IDisposable)?.Dispose();
            }
        }
    }
}