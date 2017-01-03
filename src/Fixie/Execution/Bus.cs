namespace Fixie.Execution
{
    public class Bus
    {
        readonly Listener listener;

        public Bus(Listener listener)
        {
            this.listener = listener;
        }

        public void Publish<TMessage>(TMessage message) where TMessage : Message
            => (listener as Handler<TMessage>)?.Handle(message);
    }
}