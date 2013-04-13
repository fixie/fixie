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
            var convention = new DefaultConvention();
            return Run(testListener, convention, assembly.GetTypes());
        }

        public TestRunState RunNamespace(ITestListener testListener, Assembly assembly, string ns)
        {
            var convention = new DefaultConvention();
            return Run(testListener, convention, assembly.GetTypes().Where(InNamespace(ns)).ToArray());
        }

        public TestRunState RunMember(ITestListener testListener, Assembly assembly, MemberInfo member)
        {
            var convention = new DefaultConvention();

            var method = member as MethodInfo;
            if (method != null)
            {
                convention.Cases.Where(m => m == method);

                return Run(testListener, convention, method.DeclaringType);
            }

            var type = member as Type;
            if (type != null)
                return Run(testListener, convention, type);

            return TestRunState.Error;
        }

        static TestRunState Run(ITestListener testListener, Convention convention, params Type[] candidateTypes)
        {
            var listener = new TestDrivenListener(testListener);
            var suite = new Suite(convention, candidateTypes);
            suite.Execute(listener);

            var result = listener.State.ToResult();

            if (result.Total == 0)
                return TestRunState.NoTests;

            if (result.Failed > 0)
                return TestRunState.Failure;

            return TestRunState.Success;
        }

        static Func<Type, bool> InNamespace(string ns)
        {
            return type => type.Namespace != null && type.Namespace.StartsWith(ns);
        }
    }
}