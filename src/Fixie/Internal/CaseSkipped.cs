namespace Fixie.Internal
{
    using System;

    public class CaseSkipped : CaseCompleted
    {
        public CaseSkipped(Case @case, TimeSpan duration)
            : base(@case, duration)
        {
            Reason = @case.SkipReason;
        }

        public string? Reason { get; }
    }
}