namespace Fixie.Samples.Shuffle
{
    using System;

    public class CustomConvention : Convention
    {
        const int Seed = 8675309;

        public CustomConvention()
        {
            Classes
                .InTheSameNamespaceAs(typeof(CustomConvention))
                .NameEndsWith("Tests");

            ClassExecution
                .Lifecycle<CreateInstancePerClass>()
                .ShuffleMethods(new Random(Seed));
        }
    }
}