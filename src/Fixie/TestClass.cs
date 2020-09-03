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
        internal TestClass(Type type, IReadOnlyList<TestMethod> tests, MethodInfo? targetMethod)
        {
            Type = type;
            Tests = tests;
            TargetMethod = targetMethod;
        }

        /// <summary>
        /// The test class under execution.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// The test methods under execution.
        /// </summary>
        public IReadOnlyList<TestMethod> Tests { get; }

        /// <summary>
        /// Gets the target MethodInfo identified by the
        /// test runner as the sole method to be executed.
        /// Null under normal test execution.
        /// </summary>
        public MethodInfo? TargetMethod { get; }

        public void RunTests(Action<TestMethod> testLifecycle)
        {
            foreach (var test in Tests)
            {
                try
                {
                    testLifecycle(test);
                }
                catch (Exception exception)
                {
                    test.Fail(exception);
                }
            }
        }
    }
}