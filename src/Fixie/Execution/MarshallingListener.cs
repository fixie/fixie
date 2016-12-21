namespace Fixie.Execution
{
    public class MarshallingListener : LongLivedMarshalByRefObject, Listener
    {
        readonly Listener listener;

        public MarshallingListener(Listener listener)
        {
            this.listener = listener;
        }

        public void Handle(AssemblyStarted message)
            => listener.Handle(message);

        public void Handle(CaseSkipped message)
            => listener.Handle(message);

        public void Handle(CasePassed message)
            => listener.Handle(message);

        public void Handle(CaseFailed message)
            => listener.Handle(message);

        public void Handle(AssemblyCompleted message)
            => listener.Handle(message);
    }
}