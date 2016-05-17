using System;
using Fixie.Internal;

namespace Fixie.Execution
{
    [Serializable]
    public class CaseFailed : CaseCompleted
    {
        public CaseFailed(Case @case, AssertionLibraryFilter filter)
            : base(
                methodGroup: @case.MethodGroup,
                name: @case.Name,
                status: CaseStatus.Failed,
                duration: @case.Duration,
                output: @case.Output
                )
        {
            Exception = new CompoundException(@case.Exceptions, filter);
        }

        public CompoundException Exception { get; }
    }
}