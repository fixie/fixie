namespace Fixie.Execution
{
    public class CasePassed : CaseCompleted
    {
        public CasePassed(Case @case)
            : base(
                @class: @case.Class,
                method: @case.Method,
                name: @case.Name,
                status: CaseStatus.Passed,
                duration: @case.Duration,
                output: @case.Output
                )
        {
        }
    }
}