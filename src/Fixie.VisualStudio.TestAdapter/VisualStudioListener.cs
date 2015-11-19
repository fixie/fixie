using Fixie.Execution;

namespace Fixie.VisualStudio.TestAdapter
{
    public class VisualStudioListener : IHandler<CaseResult>
    {
        readonly IExecutionSink log;

        public VisualStudioListener(IExecutionSink log)
        {
            this.log = log;
        }

        public void Handle(CaseResult message)
        {
            log.Handle(message);
        }
    }
}