using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fixie.Internal
{
    public class MethodDiscoverer
    {
        readonly Func<MethodInfo, bool>[] testMethodConditions;
        readonly BindingFlags methodFlags;

        public MethodDiscoverer(Configuration config)
        {
            testMethodConditions = config.TestMethodConditions.ToArray();
            methodFlags = config.MethodFlags;
        }

        public IReadOnlyList<MethodInfo> TestMethods(Type testClass)
        {
            try
            {
                return testClass.GetMethods(methodFlags).Where(IsMatch).ToArray();
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
