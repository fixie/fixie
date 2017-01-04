namespace Fixie.Execution
{
    using System;

    [Serializable]
    public class CaseSkipped : CaseCompleted
    {
        public CaseSkipped(Case @case, string skipReason)
            : base(
                  status: CaseStatus.Skipped,
                  name: @case.Name,
                  methodGroup: @case.MethodGroup,
                  output: null,
                  duration: TimeSpan.Zero,
                  exceptions: null,
                  skipReason: skipReason
                  )
        {
        }
    }
}