namespace Fixie.Samples.Async
{
    using System;

    public class CustomConvention : Convention, Lifecycle
    {
        public CustomConvention()
        {
            Classes
                .Where(x => x.IsInNamespace(GetType().Namespace))
                .Where(x => x.Name.EndsWith("Tests"));

            Methods
                .Where(x => x.Name != "SetUp")
                .OrderBy(x => x.Name, StringComparer.Ordinal);

            Lifecycle(this);
        }

        public void Execute(TestClass testClass, Action<CaseAction> runCases)
        {
            runCases(@case =>
            {
                var instance = testClass.Construct();

                testClass.Execute(instance, "SetUp");
                @case.Execute(instance);

                instance.Dispose();
            });
        }
    }
}