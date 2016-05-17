using System;

namespace Fixie.Execution
{
    [Serializable]
    public class CaseSkipped : CaseCompleted
    {
        public CaseSkipped(Case @case, string skipReason)
            : base(
                methodGroup: @case.MethodGroup,
                name: @case.Name,
                status: CaseStatus.Skipped,
                duration: TimeSpan.Zero,
                output: null
                )
        {
            SkipReason = skipReason;
        }

        public string SkipReason { get; }
    }
}