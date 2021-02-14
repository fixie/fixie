namespace Fixie.Reports
{
    using System;
    using Internal;

    public class CaseFailed : CaseCompleted
    {
        internal CaseFailed(Case @case, TimeSpan duration, string output, Exception exception)
            : base(@case, duration, output)
        {
            Exception = exception;
        }

        public Exception Exception { get; }
    }
}