namespace Fixie.Execution
{
    using System;

    public class CaseSkipped : CaseCompleted
    {
        public CaseSkipped(Case @case, string skipReason)
            : base(
                  @class: @case.Class,
                  method: @case.Method,
                  status: CaseStatus.Skipped,
                  name: @case.Name,
                  output: null,
                  duration: TimeSpan.Zero,
                  exceptions: null,
                  skipReason: skipReason
                  )
        {
        }
    }
}