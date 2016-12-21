namespace Fixie.Execution
{
    public class MarshallingListener : LongLivedMarshalByRefObject, Listener
    {
        readonly Listener listener;

        public MarshallingListener(Listener listener)
        {
            this.listener = listener;
        }

        public void Handle(AssemblyStarted message) => Publish(message);
        public void Handle(CaseSkipped message) => Publish(message);
        public void Handle(CasePassed message) => Publish(message);
        public void Handle(CaseFailed message) => Publish(message);
        public void Handle(AssemblyCompleted message) => Publish(message);

        void Publish<TMessage>(TMessage message) where TMessage : Message
            => (listener as Handler<TMessage>)?.Handle(message);
    }
}