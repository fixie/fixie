using Fixie.Execution;

namespace Fixie.VisualStudio.TestAdapter
{
    public class VisualStudioListener :
        IHandler<CaseSkipped>,
        IHandler<CasePassed>,
        IHandler<CaseFailed>,
        IHandler<AssemblyCompleted>
    {
        readonly IExecutionSink log;

        public VisualStudioListener(IExecutionSink log)
        {
            this.log = log;
        }

        public void Handle(CaseSkipped message)
        {
            log.RecordResult(new CaseResult(message));
        }

        public void Handle(CasePassed message)
        {
            log.RecordResult(new CaseResult(message));
        }

        public void Handle(CaseFailed message)
        {
            log.RecordResult(new CaseResult(message));
        }

        public void Handle(AssemblyCompleted message)
        {
            log.SendMessage(message.Result.Summary);
        }
    }
}