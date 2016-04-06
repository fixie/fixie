using Fixie.Execution;

namespace Fixie.Internal
{
    public class Bus : LongLivedMarshalByRefObject, Listener
    {
        readonly object listener;

        public Bus(object listener) { this.listener = listener; }

        public void Handle(AssemblyInfo message)
            => Publish(message);

        public void Handle(SkipResult message)
            => Publish(message);

        public void Handle(PassResult message)
            => Publish(message);

        public void Handle(FailResult message)
            => Publish(message);

        public void Handle(AssemblyCompleted message)
            => Publish(message);

        void Publish<TMessage>(TMessage message) where TMessage : Message
            => (listener as Handler<TMessage>)?.Handle(message);
    }
}