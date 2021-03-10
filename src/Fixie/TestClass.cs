namespace Fixie
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading.Tasks;
    using Internal;

    public class TestClass
    {
        internal TestClass(Type type, IReadOnlyList<TestMethod> tests)
        {
            Type = type;
            Tests = tests;
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

        /// <summary>
        /// Emit fail results for all tests in the test class, with the given reason.
        /// </summary>
        public async Task FailAsync(Exception reason)
        {
            foreach (var test in Tests)
                await test.FailAsync(reason);
        }
    }
}