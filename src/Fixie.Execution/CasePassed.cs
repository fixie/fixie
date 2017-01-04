namespace Fixie.Execution
{
    public class CasePassed : CaseCompleted
    {
        public CasePassed(Case @case)
            : base(
                  @class: @case.Class,
                  method: @case.Method,
                  status: CaseStatus.Passed,
                  name: @case.Name,
                  output: @case.Output,
                  duration: @case.Duration,
                  exceptions: null,
                  skipReason: null
                  )
        {
        }
    }
}