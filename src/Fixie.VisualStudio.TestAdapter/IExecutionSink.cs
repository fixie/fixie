using Fixie.Execution;

namespace Fixie.VisualStudio.TestAdapter
{
    public interface IExecutionSink
    {
        void SendMessage(string message);
        void RecordResult(CaseResult caseResult);
    }
}