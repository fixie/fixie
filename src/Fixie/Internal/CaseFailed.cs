namespace Fixie.Internal
{
    using System;

    public class CaseFailed : CaseCompleted
    {
        public CaseFailed(Case @case, TimeSpan duration) : base(@case, duration)
            => Exception = @case.Exception!;

        public Exception Exception { get; }
    }
}