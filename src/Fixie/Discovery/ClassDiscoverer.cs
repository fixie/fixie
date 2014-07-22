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
            return candidates.Where(IsMatch).ToArray();
        }

        bool IsMatch(Type candidate)
        {
            return testClassConditions.All(condition => condition(candidate));
        }
    }
}