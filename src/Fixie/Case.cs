namespace Fixie
{
    using System;
    using System.Reflection;

    /// <summary>
    /// A test case being executed, representing a single call to a test method.
    /// </summary>
    public class Case
    {
        public Case(MethodInfo caseMethod, params object[] parameters)
        {
            Parameters = parameters != null && parameters.Length == 0 ? null : parameters;
            Class = caseMethod.ReflectedType;

            Method = caseMethod.TryResolveTypeArguments(parameters);

            Name = CaseNameBuilder.GetName(Class, Method, Parameters);

            Exception = null;
        }

        internal Case(Case originalCase, Exception secondaryFailureReason)
        {
            Parameters = originalCase.Parameters;
            Class = originalCase.Class;
            Method = originalCase.Method;
            Name = originalCase.Name;

            ReturnValue = null;
            Duration = TimeSpan.Zero;
            Output = null;

            Fail(secondaryFailureReason);
        }

        /// <summary>
        /// Gets the name of the test case, including any input parameters.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the test class in which this test case is defined.
        /// </summary>
        public Type Class { get; }

        /// <summary>
        /// Gets the method that defines this test case.
        /// </summary>
        public MethodInfo Method { get; }

        /// <summary>
        /// For parameterized test cases, gets the set of parameters to be passed into the test method.
        /// For zero-argument test methods, this property is null.
        /// </summary>
        public object[] Parameters { get; }

        /// <summary>
        /// Gets the exception describing this test case's failure.
        /// </summary>
        public Exception Exception { get; private set; }

        /// <summary>
        /// Include the given exception in the running test case's list of exceptions, indicating test case failure.
        /// </summary>
        public void Fail(Exception reason)
        {
            var wrapped = reason as PreservedException;

            if (wrapped != null)
                Exception = wrapped.OriginalException;
            else
                Exception = reason;
        }

        /// <summary>
        /// Clear the test case's exception, indicating test success.
        /// </summary>
        public void ClearException()
        {
            Exception = null;
        }

        /// <summary>
        /// The object returned by the invocation of the test case method.
        /// </summary>
        public object ReturnValue { get; internal set; }

        internal TimeSpan Duration { get; set; }
        internal string Output { get; set; }
        internal bool Executed { get; set; }
        internal string SkipReason { get; set; }
    }
}
