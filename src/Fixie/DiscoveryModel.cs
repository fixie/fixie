using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fixie.Conventions;

namespace Fixie
{
    public class DiscoveryModel
    {
        readonly Func<Type, bool>[] testClassConditions;
        readonly Func<MethodInfo, bool>[] testMethodConditions;

        public DiscoveryModel(ConfigModel config)
        {
            testClassConditions = config.TestClassConditions.ToArray();
            testMethodConditions = config.TestMethodConditions.ToArray();
        }

        public IEnumerable<Type> TestClasses(IEnumerable<Type> candidates)
        {
            return candidates.Where(IsMatch).ToArray();
        }

        bool IsMatch(Type candidate)
        {
            return testClassConditions.All(condition => condition(candidate));
        }

        public IEnumerable<MethodInfo> TestMethods(Type testClass)
        {
            return testClass.GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(IsMatch).ToArray();
        }

        bool IsMatch(MethodInfo candidate)
        {
            return testMethodConditions.All(condition => condition(candidate));
        }
    }
}