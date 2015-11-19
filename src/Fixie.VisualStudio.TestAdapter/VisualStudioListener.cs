using Fixie.Execution;

namespace Fixie.VisualStudio.TestAdapter
{
    public class VisualStudioListener : IHandler<CaseResult>
    {
        readonly VisualStudioListenerProxy log;

        public VisualStudioListener(VisualStudioListenerProxy log)
        {
            this.log = log;
        }

        public void Handle(CaseResult message)
        {
            log.Handle(message);
        }
    }
}