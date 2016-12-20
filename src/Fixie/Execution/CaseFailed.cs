namespace Fixie.Execution
{
    using System;
    using Internal;

    [Serializable]
    public class CaseFailed : CaseCompleted
    {
        public CaseFailed(Case @case, AssertionLibraryFilter filter)
            : base(
                  status: CaseStatus.Failed,
                  name: @case.Name,
                  methodGroup: @case.MethodGroup,
                  output: @case.Output,
                  duration: @case.Duration,
                  exceptions: new CompoundException(@case.Exceptions, filter),
                  skipReason: null
                  )
        {
        }
    }
}