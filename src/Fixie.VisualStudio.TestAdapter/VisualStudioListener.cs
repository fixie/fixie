using System.Reflection;
using Fixie.Execution;

namespace Fixie.VisualStudio.TestAdapter
{
    public class VisualStudioListener : Listener
    {
        readonly IExecutionSink log;

        public VisualStudioListener(IExecutionSink log)
        {
            this.log = log;
        }

        public void AssemblyStarted(AssemblyStarted message) { }

        public void CaseSkipped(SkipResult result)
        {
            log.RecordResult(new CaseResult(result));
        }

        public void CasePassed(PassResult result)
        {
            log.RecordResult(new CaseResult(result));
        }

        public void CaseFailed(FailResult result)
        {
            log.RecordResult(new CaseResult(result));
        }

        public void AssemblyCompleted(AssemblyCompleted message)
        {
            log.SendMessage(message.Result.Summary);
        }
    }
}