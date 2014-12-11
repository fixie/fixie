using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fixie.Conventions;

namespace Fixie.Internal
{
    public class MethodDiscoverer
    {
        readonly Func<MethodInfo, bool>[] testMethodConditions;

        public MethodDiscoverer(Configuration config)
        {
            testMethodConditions = config.TestMethodConditions.ToArray();
        }

        public IReadOnlyList<MethodInfo> TestMethods(Type testClass)
        {
            try
            {
                return testClass.GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(IsMatch).ToArray();
            }
            catch (Exception exception)
            {
                throw new Exception(
                    "Exception thrown while attempting to run a custom method-discovery predicate. " +
                    "Check the inner exception for more details.", exception);
            }
        }

        bool IsMatch(MethodInfo candidate)
        {
            return testMethodConditions.All(condition => condition(candidate));
        }
    }
}