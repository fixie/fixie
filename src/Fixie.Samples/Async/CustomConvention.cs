namespace Fixie.Samples.Async
{
    using System;
    using System.Linq;
    using System.Reflection;

    public class CustomConvention : Convention
    {
        public CustomConvention()
        {
            Classes
                .Where(x => x.IsInNamespace(GetType().Namespace))
                .Where(x => x.Name.EndsWith("Tests"));

            Methods
                .Where(x => x.Name != "SetUp")
                .OrderBy(x => x.Name, StringComparer.Ordinal);
        }

        public override void Execute(TestClass testClass)
        {
            testClass.RunCases(@case =>
            {
                var instance = testClass.Construct();

                SetUp(instance);

                @case.Execute(instance);

                instance.Dispose();
            });
        }

        static void SetUp(object instance)
        {
            var query = instance.GetType()
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                .Where(x => x.Name == "SetUp");

            foreach (var q in query)
                q.Execute(instance);
        }
    }
}