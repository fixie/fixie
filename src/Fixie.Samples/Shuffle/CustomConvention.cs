using Fixie.Conventions;
using System;
using System.Linq;

namespace Fixie.Samples.Shuffle
{
    public class CustomConvention : Convention
    {
        public CustomConvention(RunContext runContext)
        {
            var customSeed = runContext.Options["seed"].LastOrDefault();
            int seed = Environment.TickCount;
            if (customSeed != null)
            {
                seed = Int32.Parse(customSeed);
            }
            Console.WriteLine("Running shuffled tests using seed: {0}", seed);

            Classes
                .Where(type => type.IsInNamespace(GetType().Namespace));

            Methods
                .Where(method => method.IsVoid());

            ClassExecution.ShuffleCases(new Random(seed));
        }
    }
}
