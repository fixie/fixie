namespace Fixie.Samples.Skipped
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
                .OrderBy(x => x.Name, StringComparer.Ordinal);

            ClassExecution
                .Lifecycle<SkipLifecycle>();
        }

        class SkipLifecycle : Lifecycle
        {
            public void Execute(TestClass testClass, Action<CaseAction> runCases)
            {
                var skipClass = testClass.Type.Has<SkipAttribute>();

                var instance = skipClass ? null : testClass.Construct();

                runCases(@case =>
                {
                    var skipMethod = @case.Method.Has<SkipAttribute>();

                    if (skipClass)
                        @case.Skip("Whole class skipped");
                    else if (!skipMethod)
                        @case.Execute(instance);
                });

                instance.Dispose();
            }
        }
    }
}