namespace Fixie.Execution
{
    using Internal;

    public class CaseFailed : CaseCompleted
    {
        public CaseFailed(Case @case, AssertionLibraryFilter filter)
            : base(
                  @class: @case.Class,
                  method: @case.Method,
                  status: CaseStatus.Failed,
                  name: @case.Name,
                  output: @case.Output,
                  duration: @case.Duration,
                  exceptions: new CompoundException(@case.Exceptions, filter),
                  skipReason: null
                  )
        {
        }
    }
}