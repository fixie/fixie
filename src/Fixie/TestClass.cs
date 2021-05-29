namespace Fixie
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading.Tasks;
    using Internal;

    public class TestClass
    {
        static readonly object[] EmptyParameters = {};

        internal TestClass(Type type, IReadOnlyList<Test> tests)
        {
            Type = type;
            Tests = tests;
        }

        /// <summary>
        /// The test class under execution.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// The tests under execution as discovered on this test class.
        /// </summary>
        public IReadOnlyList<Test> Tests { get; }

        /// <summary>
        /// Constructs an instance of the test class using
        /// the default constructor.
        /// </summary>
        public object Construct()
        {
            return Construct(EmptyParameters);
        }

        /// <summary>
        /// Constructs an instance of the test class using
        /// the constructor that best matches the specified
        /// parameters.
        /// </summary>
        public object Construct(object?[] parameters)
        {
            object? instance;

            try
            {
                instance = Activator.CreateInstance(Type, parameters);
            }
            catch (TargetInvocationException exception)
            {
                throw new PreservedException(exception);
            }

            if (instance == null)
            {
                throw new InvalidOperationException(
                    "An attempt to construct an instance of test class '" +
                    Type.FullName +
                    "' unexpectedly resulted in null.");
                ;
            }

            return instance;
        }

        /// <summary>
        /// Emits failure results for all tests in the test class, with the given reason.
        /// </summary>
        public async Task FailAsync(Exception reason)
        {
            foreach (var test in Tests)
                await test.FailAsync(reason);
        }
    }
}