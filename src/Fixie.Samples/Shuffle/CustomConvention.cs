namespace Fixie.Samples.Shuffle
{
    using System;

    public class CustomConvention : Convention
    {
        const int Seed = 8675309;

        public CustomConvention()
        {
            Methods
                .Shuffle(new Random(Seed));

            Classes
                .InTheSameNamespaceAs(typeof(CustomConvention))
                .NameEndsWith("Tests");

            Lifecycle<CreateInstancePerClass>();
        }
    }
}