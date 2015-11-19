using Fixie.Execution;

namespace Fixie.VisualStudio.TestAdapter
{
    public class VisualStudioListener : IHandler<CaseResult>
    {
        readonly ExecutionSink log;

        public VisualStudioListener(ExecutionSink log)
        {
            this.log = log;
        }

        public void Handle(CaseResult message)
        {
            log.Handle(message);
        }
    }
}