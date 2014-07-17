using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fixie.Conventions;

namespace Fixie.Discovery
{
    public class ClassDiscoverer
    {
        readonly Func<Type, bool>[] testClassConditions;

        public ClassDiscoverer(ConfigModel config)
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

    public class CaseDiscoverer
    {
        readonly Func<MethodInfo, bool>[] testMethodConditions;
        readonly Func<MethodInfo, IEnumerable<object[]>> getCaseParameters;

        public CaseDiscoverer(ConfigModel config)
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

        public IReadOnlyList<Case> TestCases(Type testClass)
        {
            return TestMethods(testClass).SelectMany(CasesForMethod).ToArray();
        }

        IEnumerable<Case> CasesForMethod(MethodInfo method)
        {
            var casesForKnownInputParameters = getCaseParameters(method)
                .Select(parameters => new Case(method, parameters));

            bool any = false;

            foreach (var actualCase in casesForKnownInputParameters)
            {
                any = true;
                yield return actualCase;
            }

            if (!any)
                yield return new Case(method);
        }
    }
}