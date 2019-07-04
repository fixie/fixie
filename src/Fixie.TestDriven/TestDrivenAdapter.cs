using TestDriven.Framework;

namespace Fixie.TestDriven
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Internal;

    public class TestDrivenAdapter : ITestRunner
    {
        public TestRunState RunAssembly(ITestListener testListener, Assembly assembly)
        {
            return Run(testListener, runner => runner.Run(assembly));
        }

        public TestRunState RunNamespace(ITestListener testListener, Assembly assembly, string ns)
        {
            var candidateTypes = assembly.GetTypes().Where(type => type.IsInNamespace(ns)).ToArray();

            return Run(testListener, runner => runner.Run(assembly, candidateTypes));
        }

        public TestRunState RunMember(ITestListener testListener, Assembly assembly, MemberInfo member)
        {
            if (member is MethodInfo method)
            {
                return Run(testListener, runner =>
                {
                    var tests = new[] { new Test(method) };

                    return runner.Run(assembly, tests);
                });
            }

            if (member is Type type)
            {
                var candidateTypes = GetTypeAndNestedTypes(type).ToArray();
                return Run(testListener, runner => runner.Run(assembly, candidateTypes));
            }

            return TestRunState.Error;
        }

        static IEnumerable<Type> GetTypeAndNestedTypes(Type type)
        {
            yield return type;

            foreach (var nested in type.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic).SelectMany(GetTypeAndNestedTypes))
                yield return nested;
        }

        static TestRunState Run(ITestListener testListener, Func<Runner, ExecutionSummary> run)
        {
            var listener = new TestDrivenListener(testListener);
            var bus = new Bus(listener);

            var runner = new Runner(bus);
            
            var summary = run(runner);

            if (summary.Total == 0)
                return TestRunState.NoTests;

            if (summary.Failed > 0)
                return TestRunState.Failure;

            return TestRunState.Success;
        }
    }
}