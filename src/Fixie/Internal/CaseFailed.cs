namespace Fixie.Internal
{
    using System;

    public class CaseFailed : CaseCompleted
    {
        public CaseFailed(Case @case) : base(@case)
            => Exception = @case.Exception;

        public Exception Exception { get; }
    }
}