using System;
using Fixie.Internal;

namespace Fixie.Execution
{
    [Serializable]
    public class CaseFailed : CaseCompleted
    {
        public CaseFailed(Case @case, AssertionLibraryFilter filter)
            : this(@case, new CompoundException(@case.Exceptions, filter))
        {
        }

        CaseFailed(Case @case, CompoundException exception)
            : base(
                methodGroup: @case.MethodGroup,
                name: @case.Name,
                status: CaseStatus.Failed,
                duration: @case.Duration,
                output: @case.Output,
                exceptions: exception
                )
        {
        }
    }
}