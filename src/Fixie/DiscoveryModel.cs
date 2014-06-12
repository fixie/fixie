using System;
using System.Collections.Generic;
using System.Linq;
using Fixie.Conventions;

namespace Fixie
{
    public class DiscoveryModel
    {
        readonly Func<Type, bool>[] testClassConditions;

        public DiscoveryModel(ConfigModel config)
        {
            testClassConditions = config.TestClassConditions.ToArray();
        }

        public IEnumerable<Type> TestClasses(IEnumerable<Type> candidates)
        {
            return candidates.Where(IsMatch).ToArray();
        }

        bool IsMatch(Type candidate)
        {
            return testClassConditions.All(condition => condition(candidate));
        }
    }
}