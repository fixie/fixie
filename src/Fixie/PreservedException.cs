namespace Fixie
{
    using System;

    /// <summary>
    /// Represents an original exception that has surfaced via a reflection call such as calling MethodInfo.Invoke.
    /// The reflection API wraps the root cause of a failed method invocation by wrapping it in a type such as TargetInvocationException.
    /// Instead of reporting a reflection implementation detail like TargetInvocationException as a test case failure, and
    /// instead of mistakenly rethrowing the original InnerException (which tramples the stack trace), wrap the original
    /// InnerException in a PreservedException and throw it.  When a PreservedException is reported to Fixie as the reason
    /// for a test failure, the original exception will be unpacked and reported as the actual cause of the test case failure.
    /// </summary>
    class PreservedException : Exception
    {
        public PreservedException(Exception originalException)
        {
            OriginalException = originalException;
        }

        public Exception OriginalException { get; }
    }
}
