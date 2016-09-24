namespace Fixie.Execution.Listeners
{
    public class SummaryListener : Handler<CaseCompleted>
    {
        public ExecutionSummary Summary { get; } = new ExecutionSummary();

        public void Handle(CaseCompleted message) => Summary.Add(message);
    }
}