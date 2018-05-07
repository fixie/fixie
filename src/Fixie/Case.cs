namespace Fixie
{
    using System;
    using System.Reflection;
    using Internal;

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

            Output = "";
        }

        internal Case(Case originalCase, Exception secondaryFailureReason)
        {
            Parameters = originalCase.Parameters;
            Class = originalCase.Class;
            Method = originalCase.Method;
            Name = originalCase.Name;
            Output = "";

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
        /// Indicate the test case was skipped for the given reason.
        /// </summary>
        public void Skip(string reason)
        {
            State = CaseState.Skipped;
            Exception = null;
            SkipReason = reason;
        }

        /// <summary>
        /// Indicate the test case passed.
        /// </summary>
        public void Pass()
        {
            State = CaseState.Passed;
            Exception = null;
            SkipReason = null;
        }

        /// <summary>
        /// Indicate the test case failed for the given reason.
        /// </summary>
        public void Fail(Exception reason)
        {
            State = CaseState.Failed;

            if (reason is PreservedException wrapped)
                Exception = wrapped.OriginalException;
            else
                Exception = reason;

            SkipReason = null;

            if (reason == null)
                Fail("The custom test class lifecycle did not provide an Exception for this test case failure.");
        }

        /// <summary>
        /// Indicate the test case failed for the given reason.
        /// </summary>
        public void Fail(string reason)
        {
            try
            {
                throw new Exception(reason);
            }
            catch (Exception exception)
            {
                Fail(exception);
            }
        }

        internal TimeSpan Duration { get; set; }
        internal string Output { get; set; }
        internal string SkipReason { get; private set; }
        internal CaseState State { get; private set; }

        /// <summary>
        /// Execute the test case against the given instance of the test class,
        /// causing the case state to become either passing or failing.
        /// </summary>
        /// <returns>
        /// For void methods, returns null.
        /// For synchronous methods, returns the value returned by the test method.
        /// For async Task methods, returns null after awaiting the Task.
        /// For async Task<![CDATA[<T>]]> methods, returns the Result T after awaiting the Task.
        /// </returns>
        public object Execute(object instance)
        {
            try
            {
                var result = Method.Execute(instance, Parameters);
                Pass();
                return result;
            }
            catch (Exception exception)
            {
                Fail(exception);
                return null;
            }
        }
    }

    enum CaseState
    {
        Skipped,
        Passed,
        Failed
    }
}
