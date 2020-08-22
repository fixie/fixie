namespace Fixie
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.ExceptionServices;
    using Internal;

    /// <summary>
    /// The context in which a test class is running.
    /// </summary>
    public class TestClass
    {
        readonly ExecutionRecorder recorder;
        readonly IReadOnlyList<TestMethod> testMethods;
        readonly bool isStatic;

        internal TestClass(ExecutionRecorder recorder, Type type, IReadOnlyList<TestMethod> testMethods, MethodInfo? targetMethod)
        {
            this.recorder = recorder;
            this.testMethods = testMethods;

            Type = type;
            TargetMethod = targetMethod;
            isStatic = Type.IsStatic();
            Invoked = false;
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

        internal bool Invoked { get; private set; }

        /// <summary>
        /// Constructs an instance of the test class type, using its default constructor.
        /// If the class is static, no action is taken and null is returned.
        /// </summary>
        public object? Construct()
        {
            if (isStatic)
                return null;

            try
            {
                return Activator.CreateInstance(Type);
            }
            catch (TargetInvocationException exception)
            {
                ExceptionDispatchInfo.Capture(exception.InnerException!).Throw();
                throw; //Unreachable.
            }
        }

        public void RunTests(Action<TestMethod> testLifecycle)
        {
            Invoked = true;

            foreach (var testMethod in testMethods)
            {
                recorder.Start(testMethod);

                try
                {
                    testLifecycle(testMethod);

                    if (!testMethod.Invoked)
                        recorder.Skip(testMethod);
                }
                catch (Exception exception)
                {
                    recorder.Fail(testMethod, exception);
                }
            }
        }
    }
}