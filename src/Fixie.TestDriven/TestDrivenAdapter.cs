using TestDriven.Framework;

namespace Fixie.TestDriven
{
    using System;
    using System.Reflection;
    using Execution;

    public class TestDrivenAdapter : ITestRunner
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
            if (member is MethodInfo method)
            {
                var testClass = method.DeclaringType;

                return Run(testListener, runner => runner.RunMethods(assembly, testClass, method));
            }

            if (member is Type type)
                return Run(testListener, runner => runner.RunType(assembly, type));

            return TestRunState.Error;
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