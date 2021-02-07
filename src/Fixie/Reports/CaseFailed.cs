namespace Fixie.Reports
{
    using System;
    using Internal;

    public class CaseFailed : CaseCompleted
    {
        internal CaseFailed(Case @case, TimeSpan duration, string output) : base(@case, duration, output)
            => Exception = @case.Exception!;

        public Exception Exception { get; }
    }
}