using System;
using System.Collections.Generic;
using System.Linq;
using Fixie.Conventions;

namespace Fixie.Discovery
{
    public class ClassDiscoverer
    {
        readonly Func<Type, bool>[] testClassConditions;

        public ClassDiscoverer(Configuration config)
        {
            testClassConditions = config.TestClassConditions.ToArray();
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