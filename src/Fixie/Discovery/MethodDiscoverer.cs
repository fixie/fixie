using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fixie.Conventions;

namespace Fixie.Discovery
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
            return testClass.GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(IsMatch).ToArray();
        }

        bool IsMatch(MethodInfo candidate)
        {
            return testMethodConditions.All(condition => condition(candidate));
        }
    }
}