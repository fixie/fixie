using System;

namespace Fixie.Execution
{
    [Serializable]
    public class CaseSkipped : CaseCompleted
    {
        public CaseSkipped(Case @case, string reason)
            : base(
                methodGroup: @case.MethodGroup,
                name: @case.Name,
                status: CaseStatus.Skipped,
                duration: TimeSpan.Zero,
                output: null
                )
        {
            Reason = reason;
        }

        public string Reason { get; }
    }
}