namespace Fixie.Execution
{
    using System;
    using Listeners;

    public class CaseFailed : CaseCompleted
    {
        public CaseFailed(Case @case) : base(@case)
            => Exception = @case.Exception;

        public Exception Exception { get; }
    }
}