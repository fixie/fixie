namespace Fixie.Execution
{
    public class CaseSkipped : CaseCompleted
    {
        public CaseSkipped(Case @case)
            : base(@case)
        {
            Reason = @case.SkipReason;
        }

        public string Reason { get; }
    }
}