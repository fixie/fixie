using System;

namespace Fixie.Execution
{
    [Serializable]
    public class CaseSkipped : CaseCompleted
    {
        public CaseSkipped(Case @case, string skipReason)
        {
            MethodGroup = @case.MethodGroup;
            Name = @case.Name;
            Status = CaseStatus.Skipped;
            Duration = TimeSpan.Zero;
            Output = null;
            Exceptions = null;
            SkipReason = skipReason;
        }
    }
}