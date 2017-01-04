namespace Fixie.Execution
{
    public class CasePassed : CaseCompleted
    {
        public CasePassed(Case @case)
            : base(
                  status: CaseStatus.Passed,
                  name: @case.Name,
                  methodGroup: @case.MethodGroup,
                  output: @case.Output,
                  duration: @case.Duration,
                  exceptions: null,
                  skipReason: null
                  )
        {
        }
    }
}