namespace Fixie.Internal
{
    using System;
    using Execution;

    public static class CompoundExceptionExtensions
    {
        public static string TypedStackTrace(this CompoundException exception)
        {
            if (exception.FailedAssertion)
                return exception.StackTrace;

            return exception.Type + Environment.NewLine + exception.StackTrace;
        }
    }
}