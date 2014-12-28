using System;

namespace Fixie.Samples.Shuffle
{
    public class CustomConvention : Convention
    {
        const int Seed = 8675309;

        public CustomConvention()
        {
            Classes
                .InTheSameNamespaceAs(typeof(CustomConvention))
                .NameEndsWith("Tests");

            Methods
                .Where(method => method.IsVoid());

            ClassExecution
                .CreateInstancePerClass()
                .ShuffleCases(new Random(Seed));
        }
    }
}