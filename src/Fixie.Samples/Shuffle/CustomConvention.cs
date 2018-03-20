namespace Fixie.Samples.Shuffle
{
    using System;

    public class CustomConvention : Convention, Lifecycle
    {
        const int Seed = 8675309;

        public CustomConvention()
        {
            Methods
                .Shuffle(new Random(Seed));

            Classes
                .Where(x => x.IsInNamespace(GetType().Namespace))
                .Where(x => x.Name.EndsWith("Tests"));

            Lifecycle(this);
        }

        public void Execute(TestClass testClass, Action<CaseAction> runCases)
        {
            var instance = testClass.Construct();

            runCases(@case => @case.Execute(instance));

            instance.Dispose();
        }
    }
}