namespace Fixie.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class ClassDiscoverer
    {
        readonly Func<Type, bool>[] testClassConditions;

        public ClassDiscoverer(Convention convention)
        {
            testClassConditions = convention.Config.TestClassConditions.ToArray();
        }

        public IReadOnlyList<Type> TestClasses(IEnumerable<Type> candidates)
        {
            try
            {
                return candidates.Where(IsMatch).ToArray();
            }
            catch (Exception exception)
            {
                throw new Exception(
                    "Exception thrown while attempting to run a custom class-discovery predicate. " +
                    "Check the inner exception for more details.", exception);
            }
        }

        bool IsMatch(Type candidate)
        {
            return testClassConditions.All(condition => condition(candidate));
        }
    }
}