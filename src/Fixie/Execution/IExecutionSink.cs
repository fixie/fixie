namespace Fixie.Execution
{
    public interface IExecutionSink
    {
        void RecordResult(CaseResult caseResult);
    }
}