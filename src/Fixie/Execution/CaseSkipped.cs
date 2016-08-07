namespace Fixie.Execution
{
    public class CaseSkipped : CaseCompleted
    {
        public CaseSkipped(Case @case, string reason)
            : base(
                methodGroup: @case.MethodGroup,
                name: @case.Name,
                status: CaseStatus.Skipped,
                duration: @case.Duration,
                output: @case.Output
                )
        {
            Reason = reason;
        }

        public string Reason { get; }
    }
}