using Fixie.Execution;

namespace Fixie.VisualStudio.TestAdapter
{
    public class ListenerFactory : IListenerFactory
    {
        readonly IExecutionSink executionSink;

        public ListenerFactory(IExecutionSink executionSink)
        {
            this.executionSink = executionSink;
        }

        public Listener Create(Options options)
        {
            return new VisualStudioListener(executionSink);
        }
    }
}