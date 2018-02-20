namespace Fixie.Tests.Execution.Listeners
{
    using System;
    using Fixie.Execution.Listeners;

    public class ExceptionExtensionsTests
    {
        public void ShouldGetCompoundStackTraceIncludingAllNestedExceptions()
        {
            var exception = GetException();

            exception.CompoundStackTrace()
                .CleanStackTraceLineNumbers()
                .Lines()
                .ShouldEqual(
                    Utility.At<ExceptionExtensionsTests>("GetException()"),
                    "",
                    "------- Inner Exception: System.DivideByZeroException -------",
                    "Divide by Zero Exception!",
                    Utility.At<ExceptionExtensionsTests>("GetException()"));
        }

        static Exception GetException()
        {
            try
            {
                try
                {
                    throw new DivideByZeroException("Divide by Zero Exception!");
                }
                catch (Exception exception)
                {
                    throw new PrimaryException(exception);
                }
            }
            catch (Exception exception)
            {
                return exception;
            }
        }

        class PrimaryException : Exception
        {
            public PrimaryException(Exception innerException)
                : base("Primary Exception!", innerException) { }
        }
    }
}
