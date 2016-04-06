using Fixie.Execution;

namespace Fixie.Internal
{
    public class MarshallingListener : LongLivedMarshalByRefObject, Listener
    {
        readonly Listener listener;

        public MarshallingListener(Listener listener) { this.listener = listener; }

        public void Handle(AssemblyInfo message) => listener.Handle(message);
        public void Handle(SkipResult message) => listener.Handle(message);
        public void Handle(PassResult message) => listener.Handle(message);
        public void Handle(FailResult message) => listener.Handle(message);
        public void Handle(AssemblyCompleted message) => listener.Handle(message);
    }
}