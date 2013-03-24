using System;
using System.Linq;
using System.Reflection;
using TestDriven.Framework;

namespace Fixie.TestDriven
{
    public class Runner : ITestRunner
    {
        public TestRunState RunAssembly(ITestListener testListener, Assembly assembly)
        {
            return Run(testListener, assembly.GetTypes());
        }

        public TestRunState RunNamespace(ITestListener testListener, Assembly assembly, string ns)
        {
            return Run(testListener, assembly.GetTypes().Where(InNamespace(ns)).ToArray());
        }

        public TestRunState RunMember(ITestListener testListener, Assembly assembly, MemberInfo member)
        {
            var method = member as MethodInfo;
            if (method != null)
            {
                //TODO: Rather than executing all cases in the method.ReflectedType,
                //      need a way to limit the fixture to only consider the case(s)
                //      for the specific method.  Modify the convention?

                return Run(testListener, method.ReflectedType);
            }

            var type = member as Type;
            if (type != null)
                return Run(testListener, type);

            return TestRunState.Error;
        }

        static TestRunState Run(ITestListener testListener, params Type[] candidateTypes)
        {
            var listener = new TestDrivenListener(testListener);
            var suite = new Suite(candidateTypes);
            var result = suite.Execute(listener);

            return RunState(result);
        }

        static Func<Type, bool> InNamespace(string ns)
        {
            return type => type.Namespace != null && type.Namespace.StartsWith(ns);
        }

        static TestRunState RunState(Result result)
        {
            if (result.Total == 0)
                return TestRunState.NoTests;

            if (result.Failed > 0)
                return TestRunState.Failure;

            return TestRunState.Success;
        }
    }
}