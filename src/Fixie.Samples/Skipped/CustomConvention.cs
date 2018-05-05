namespace Fixie.Samples.Skipped
{
    using System;

    public class CustomConvention : Convention
    {
        public CustomConvention()
        {
            Classes
                .Where(x => x.IsInNamespace(GetType().Namespace))
                .Where(x => x.Name.EndsWith("Tests"));

            Methods
                .OrderBy(x => x.Name, StringComparer.Ordinal);
        }

        public override void Execute(TestClass testClass)
        {
            var methodWasExplicitlyRequested = testClass.TargetMethod != null;

            var skipClass = testClass.Type.Has<SkipAttribute>() && !methodWasExplicitlyRequested;

            var instance = skipClass ? null : testClass.Construct();

            testClass.RunCases(@case =>
            {
                var skipMethod = @case.Method.Has<SkipAttribute>() && !methodWasExplicitlyRequested;

                if (skipClass)
                    @case.Skip("Whole class skipped");
                else if (!skipMethod)
                    @case.Execute(instance);
            });

            instance.Dispose();
        }
    }
}