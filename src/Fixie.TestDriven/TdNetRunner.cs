using TestDriven.Framework;

namespace Fixie.TestDriven
{
    using System;
    using System.Reflection;
    using Execution;

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
            {
                if (method.IsDispose())
                {
                    var listener = new TestDrivenListener(testListener);
                    listener.Handle(new CaseSkipped(new Case(method.DeclaringType, method), "Dispose() is not a test."));
                    return TestRunState.Success;
                }

                return Run(testListener, runner => runner.RunMethods(assembly, method));
            }

            var type = member as Type;
            if (type != null)
                return Run(testListener, runner => runner.RunType(assembly, type));

            return TestRunState.Error;
        }

        static TestRunState Run(ITestListener testListener, Action<Runner> run)
        {
            var listener = new TestDrivenListener(testListener);
            var bus = new Bus(listener);
            var runner = new Runner(bus);
            run(runner);

            var summary = listener.Summary;

            if (summary.Total == 0)
                return TestRunState.NoTests;

            if (summary.Failed > 0)
                return TestRunState.Failure;

            return TestRunState.Success;
        }
    }
}