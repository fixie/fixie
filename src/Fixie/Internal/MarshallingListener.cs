using Fixie.Execution;

namespace Fixie.Internal
{
    public class MarshallingListener : LongLivedMarshalByRefObject, Listener
    {
        readonly Listener listener;

        public MarshallingListener(Listener listener) { this.listener = listener; }

        public void Handle(AssemblyInfo assembly) => listener.Handle(assembly);
        public void Handle(SkipResult result) => listener.Handle(result);
        public void Handle(PassResult result) => listener.Handle(result);
        public void Handle(FailResult result) => listener.Handle(result);
        public void Handle(AssemblyCompleted message) => listener.Handle(message);
    }
}