namespace Fixie.Tests.Internal.Listeners
{
    using System;
    using Fixie.Internal.Listeners;
    using static Utility;

    public class ExceptionExtensionsTests
    {
        public void ShouldGetLiterateStackTraceIncludingAllNestedExceptions()
        {
            var exception = GetException();

            exception.LiterateStackTrace()
                .CleanStackTraceLineNumbers()
                .Lines()
                .ShouldEqual(
                    At<ExceptionExtensionsTests>("GetException()"),
                    "",
                    "------- Inner Exception: System.DivideByZeroException -------",
                    "Divide by Zero Exception!",
                    At<ExceptionExtensionsTests>("GetException()"));
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
