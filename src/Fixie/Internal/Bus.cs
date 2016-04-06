using Fixie.Execution;

namespace Fixie.Internal
{
    public class Bus : LongLivedMarshalByRefObject
    {
        readonly object listener;

        public Bus(object listener) { this.listener = listener; }

        public void Publish(AssemblyInfo message)
            => Publish<AssemblyInfo>(message);

        public void Publish(SkipResult message)
            => Publish<SkipResult>(message);

        public void Publish(PassResult message)
            => Publish<PassResult>(message);

        public void Publish(FailResult message)
            => Publish<FailResult>(message);

        public void Publish(AssemblyCompleted message)
            => Publish<AssemblyCompleted>(message);

        void Publish<TMessage>(TMessage message) where TMessage : Message
            => (listener as Handler<TMessage>)?.Handle(message);
    }
}