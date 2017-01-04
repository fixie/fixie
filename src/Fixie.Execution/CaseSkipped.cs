namespace Fixie.Execution
{
    using System;

    public class CaseSkipped : CaseCompleted
    {
        public CaseSkipped(Case @case, string reason)
            : base(
                @class: @case.Class,
                method: @case.Method,
                name: @case.Name,
                status: CaseStatus.Skipped,
                duration: TimeSpan.Zero,
                output: null,

                exceptions: null
                )
        {
            Reason = reason;
        }

        public string Reason { get; }
    }
}