namespace Fixie.Samples.LowCeremony
{
    using System;
    using System.Linq;
    using System.Reflection;

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
        }

        public override void Execute(TestClass testClass, Action<CaseAction> runCases)
        {
            var instance = testClass.Construct();

            void Execute(string method)
            {
                var query = testClass.Type
                    .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                    .Where(x => x.Name == method);

                foreach (var q in query)
                    q.Execute(instance);
            }

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