namespace Fixie
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public class TestClass
    {
        internal TestClass(TestAssembly testAssembly, Type type, IReadOnlyList<TestMethod> tests, MethodInfo? targetMethod)
        {
            TestAssembly = testAssembly;
            Type = type;
            Tests = tests;
            TargetMethod = targetMethod;
        }

        /// <summary>
        /// The test assembly under execution.
        /// </summary>
        public TestAssembly TestAssembly { get; }

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

        /// <summary>
        /// Construct an instance of the test class using
        /// the constructor that best matches the specified
        /// parameters.
        /// </summary>
        public object? Construct(params object?[] parameters)
        {
            try
            {
                return Activator.CreateInstance(Type, parameters);
            }
            catch (TargetInvocationException exception)
            {
                throw new PreservedException(exception);
            }
        }
    }
}