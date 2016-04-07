using Fixie.Execution;

namespace Fixie.Internal
{
    public class Bus : LongLivedMarshalByRefObject
    {
        readonly object listener;

        public Bus(object listener)
        {
            this.listener = listener;
        }

        public void Publish<TMessage>(TMessage message) where TMessage : Message
            => (listener as Handler<TMessage>)?.Handle(message);
    }
}