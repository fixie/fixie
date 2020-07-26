namespace Fixie.Internal
{
    using System;

    public class CaseFailed : CaseCompleted
    {
        public CaseFailed(Case @case, TimeSpan duration, string output) : base(@case, duration, output)
            => Exception = @case.Exception!;

        public Exception Exception { get; }
    }
}