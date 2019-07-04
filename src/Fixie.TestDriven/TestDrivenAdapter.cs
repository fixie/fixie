using TestDriven.Framework;

namespace Fixie.TestDriven
{
    using System;
    using System.Reflection;
    using Internal;

    public class TestDrivenAdapter : ITestRunner
    {
        public TestRunState RunAssembly(ITestListener testListener, Assembly assembly)
        {
            return Run(testListener, assembly, runner => runner.Run());
        }

        public TestRunState RunNamespace(ITestListener testListener, Assembly assembly, string ns)
        {
            return Run(testListener, assembly, runner => runner.Run(@class => @class.IsInNamespace(ns)));
        }

        public TestRunState RunMember(ITestListener testListener, Assembly assembly, MemberInfo member)
        {
            if (member is MethodInfo method)
                return Run(testListener, assembly, runner => runner.Run(new[] {new Test(method)}));

            if (member is Type type)
                return Run(testListener, assembly, runner => runner.Run(@class => @class == type));

            return TestRunState.Error;
        }

        static TestRunState Run(ITestListener testListener, Assembly assembly, Func<Runner, ExecutionSummary> run)
        {
            var listener = new TestDrivenListener(testListener);
            var bus = new Bus(listener);

            var runner = new Runner(assembly, bus);
            
            var summary = run(runner);

            if (summary.Total == 0)
                return TestRunState.NoTests;

            if (summary.Failed > 0)
                return TestRunState.Failure;

            return TestRunState.Success;
        }
    }
}