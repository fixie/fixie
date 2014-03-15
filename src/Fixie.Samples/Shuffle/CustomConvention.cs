using Fixie.Conventions;
using System;

namespace Fixie.Samples.Shuffle
{
    public class CustomConvention : Convention
    {
        const int Seed = 8675309;

        public CustomConvention()
        {
            Classes
                .Where(type => type.IsInNamespace(GetType().Namespace))
                .NameEndsWith("Tests");

            Methods
                .Where(method => method.IsVoid());

            ClassExecution
                .CreateInstancePerTestClass()
                .ShuffleCases(new Random(Seed));
        }
    }
}