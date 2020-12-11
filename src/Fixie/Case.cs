namespace Fixie
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading.Tasks;
    using Internal;

    /// <summary>
    /// A test case being executed, representing a single call to a test method.
    /// </summary>
    public class Case
    {
        readonly object?[] parameters;

        internal Case(MethodInfo testMethod, object?[] parameters)
        {
            this.parameters = parameters;
            Test = new Test(testMethod);
            Method = testMethod.TryResolveTypeArguments(parameters);
            Name = CaseNameBuilder.GetName(Method, parameters);
        }

        internal Case(Case originalCase, Exception secondaryFailureReason)
        {
            parameters = originalCase.parameters;
            Test = originalCase.Test;
            Method = originalCase.Method;
            Name = originalCase.Name;

            Fail(secondaryFailureReason);
        }

        /// <summary>
        /// Gets the test for which this case describes a single execution.
        /// </summary>
        public Test Test { get; }

        /// <summary>
        /// Gets the input parameters for this single execution of the test.
        /// </summary>
        public IReadOnlyList<object?> Parameters => parameters;

        /// <summary>
        /// Gets the name of the test case, including any input parameters.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the method that defines this test case.
        /// </summary>
        public MethodInfo Method { get; }

        /// <summary>
        /// Gets the exception describing this test case's failure.
        /// </summary>
        public Exception? Exception { get; private set; }

        /// <summary>
        /// Indicate the test case was skipped for the given reason.
        /// </summary>
        public void Skip(string? reason)
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
            if (reason is PreservedException preservedException)
                reason = preservedException.OriginalException;

            State = CaseState.Failed;

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

        internal string? SkipReason { get; private set; }
        internal CaseState State { get; private set; }

        /// <summary>
        /// Run the test case against the given instance of the test class,
        /// causing the case state to become either passing or failing.
        /// </summary>
        internal async Task RunAsync(object? instance)
        {
            try
            {
                await Method.ExecuteAsync(instance, parameters);
                Pass();
            }
            catch (Exception exception)
            {
                Fail(exception);
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
