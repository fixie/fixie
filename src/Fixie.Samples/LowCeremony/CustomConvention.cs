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
                .Where(x => x.IsInNamespace(GetType().Namespace))
                .Where(x => x.Name.EndsWith("Tests"));

            Methods
                .Where(x => !LifecycleMethods.Contains(x.Name))
                .OrderBy(x => x.Name, StringComparer.Ordinal);

            Lifecycle<CallSetUpTearDownMethodsByName>();
        }

        class CallSetUpTearDownMethodsByName : Lifecycle
        {
            public void Execute(TestClass testClass, Action<CaseAction> runCases)
            {
                var instance = testClass.Construct();

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

                instance.Dispose();
            }
        }
    }
}