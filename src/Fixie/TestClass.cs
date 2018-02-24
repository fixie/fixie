namespace Fixie
{
    using System;
    using System.Reflection;

    /// <summary>
    /// The context in which a test class is running.
    /// </summary>
    public class TestClass
    {
        internal TestClass(Type type) : this(type, null) { }

        internal TestClass(Type type, MethodInfo targetMethod)
        {
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
        public MethodInfo TargetMethod { get; }
    }
}