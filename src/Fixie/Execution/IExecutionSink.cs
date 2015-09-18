namespace Fixie.Execution
{
    public interface IExecutionSink
    {
        void SendMessage(string message);
        void RecordResult(CaseResult caseResult);
    }
}