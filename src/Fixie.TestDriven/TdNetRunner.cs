using System;
using System.Reflection;
using Fixie.Execution;
using Fixie.Internal;
using System.Linq;
using TestDriven.Framework;
using System.Collections.Generic;

namespace Fixie.TestDriven
{
    public class TdNetRunner : ITestRunner
    {
        public TestRunState RunAssembly(ITestListener testListener, Assembly assembly)
        {
            return Run(testListener, runner => runner.RunAssembly(assembly));
        }

        public TestRunState RunNamespace(ITestListener testListener, Assembly assembly, string ns)
        {
            return Run(testListener, runner => runner.RunNamespace(assembly, ns));
        }

        public TestRunState RunMember(ITestListener testListener, Assembly assembly, MemberInfo member)
        {
            var method = member as MethodInfo;
            if (method != null)
                return Run(testListener, runner => runner.RunMethods(assembly, method));

            var type = member as Type;
            if (type != null)
            {
                var types = GetTypeAndNestedTypes(type).ToArray();
                return Run(testListener, runner => runner.RunTypes(assembly, types));
            }

            return TestRunState.Error;
        }

        private static IEnumerable<Type> GetTypeAndNestedTypes(Type type)
        {
            yield return type;

            foreach (var nestedType in type.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic).SelectMany(t => GetTypeAndNestedTypes(t)))
            {
                yield return nestedType;
            }
        }

        public TestRunState Run(ITestListener testListener, Func<Runner, AssemblyResult> run)
        {
            var listener = new TestDrivenListener(testListener);
            var runner = new Runner(listener);
            var result = run(runner);

            if (result.Total == 0)
                return TestRunState.NoTests;

            if (result.Failed > 0)
                return TestRunState.Failure;

            return TestRunState.Success;
        }
    }
}