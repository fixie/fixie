using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fixie.Conventions;

namespace Fixie.Discovery
{
    public class CaseDiscoverer
    {
        readonly Func<MethodInfo, bool>[] testMethodConditions;
        readonly Func<MethodInfo, IEnumerable<object[]>> getCaseParameters;

        public CaseDiscoverer(Configuration config)
        {
            testMethodConditions = config.TestMethodConditions.ToArray();
            getCaseParameters = config.GetCaseParameters;
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