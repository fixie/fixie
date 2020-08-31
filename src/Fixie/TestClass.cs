namespace Fixie
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Internal;

    /// <summary>
    /// The context in which a test class is running.
    /// </summary>
    public class TestClass
    {
        readonly ExecutionRecorder recorder;
        readonly IReadOnlyList<TestMethod> testMethods;

        internal TestClass(ExecutionRecorder recorder, Type type, IReadOnlyList<TestMethod> testMethods, MethodInfo? targetMethod)
        {
            this.recorder = recorder;
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
                recorder.Start(testMethod);

                try
                {
                    testLifecycle(testMethod);

                    if (!testMethod.RecordedResult)
                        testMethod.Skip();
                }
                catch (Exception exception)
                {
                    testMethod.Fail(exception);
                }
            }
        }
    }
}