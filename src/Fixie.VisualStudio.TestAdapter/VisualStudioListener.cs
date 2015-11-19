using Fixie.Execution;

namespace Fixie.VisualStudio.TestAdapter
{
    public class VisualStudioListener :
        IHandler<CaseSkipped>,
        IHandler<CasePassed>,
        IHandler<CaseFailed>
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
    }
}