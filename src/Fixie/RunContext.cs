namespace Fixie
{
    using System;
    using System.Reflection;

    /// <summary>
    /// Describes the context in which a test class is running.
    /// </summary>
    public class RunContext
    {
        internal RunContext(Type testClass) : this(testClass, null) { }

        internal RunContext(Type testClass, MethodInfo targetMethod)
        {
            TestClass = testClass;
            TargetMethod = targetMethod;
        }

        /// <summary>
        /// The test class to execute.
        /// </summary>
        public Type TestClass { get; }

        /// <summary>
        /// Gets the target MethodInfo identified by the
        /// test runner as the sole method to be executed.
        /// Null under normal test execution.
        /// </summary>
        public MethodInfo TargetMethod { get; }
    }
}