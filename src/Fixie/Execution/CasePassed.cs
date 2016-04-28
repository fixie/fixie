using System;

namespace Fixie.Execution
{
    [Serializable]
    public class CasePassed : CaseCompleted
    {
        public CasePassed(Case @case)
        {
            MethodGroup = @case.MethodGroup;
            Name = @case.Name;
            Status = CaseStatus.Passed;
            Duration = @case.Duration;
            Output = @case.Output;
            Exceptions = null;
            SkipReason = null;
        }
    }
}