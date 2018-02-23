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
                .Where(method => LifecycleMethods.All(x => x != method.Name));

            ClassExecution
                .Lifecycle<CallSetUpTearDownMethodsByName>()
                .SortMethods((methodA, methodB) => String.Compare(methodA.Name, methodB.Name, StringComparison.Ordinal));
        }

        class CallSetUpTearDownMethodsByName : Lifecycle
        {
            public void Execute(Type testClass, Action<CaseAction> runCases)
            {
                var instance = Activator.CreateInstance(testClass);

                void Execute(string method)
                    => testClass.Execute(instance, method);

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