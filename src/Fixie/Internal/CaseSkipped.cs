namespace Fixie.Internal
{
    using System;

    public class CaseSkipped : CaseCompleted
    {
        public CaseSkipped(Case @case, TimeSpan duration, string output)
            : base(@case, duration, output)
        {
            Reason = @case.SkipReason;
        }

        public string? Reason { get; }
    }
}