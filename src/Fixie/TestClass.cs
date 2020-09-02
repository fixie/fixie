namespace Fixie
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    /// The context in which a test class is running.
    /// </summary>
    public class TestClass
    {
        readonly IReadOnlyList<TestMethod> testMethods;

        internal TestClass(Type type, IReadOnlyList<TestMethod> testMethods, MethodInfo? targetMethod)
        {
            this.testMethods = testMethods;

            Type = type;
            TargetMethod = targetMethod;
        }

        /// <summary>
        /// The test class to execute.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Gets the target MethodInfo identified by the
        /// test runner as the sole method to be executed.
        /// Null under normal test execution.
        /// </summary>
        public MethodInfo? TargetMethod { get; }

        public void RunTests(Action<TestMethod> testLifecycle)
        {
            foreach (var testMethod in testMethods)
            {
                try
                {
                    testLifecycle(testMethod);
                }
                catch (Exception exception)
                {
                    testMethod.Fail(exception);
                }
            }
        }
    }
}