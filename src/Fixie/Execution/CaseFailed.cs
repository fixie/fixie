using System;
using Fixie.Internal;

namespace Fixie.Execution
{
    [Serializable]
    public class CaseFailed : CaseCompleted
    {
        public CaseFailed(Case @case, AssertionLibraryFilter filter)
        {
            MethodGroup = @case.MethodGroup;
            Name = @case.Name;
            Status = CaseStatus.Failed;
            Duration = @case.Duration;
            Output = @case.Output;
            Exceptions = new CompoundException(@case.Exceptions, filter);
            SkipReason = null;
        }
    }
}