using Fixie.Execution;

namespace Fixie.VisualStudio.TestAdapter
{
    public class ListenerFactory : IListenerFactory
    {
        public Listener Create(Options options, IExecutionSink executionSink)
        {
            return new VisualStudioListener(executionSink);
        }
    }
}