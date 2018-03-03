namespace Fixie.Execution
{
    using System.Collections.Generic;

    class Bus
    {
        readonly List<Listener> listeners;

        public Bus(Listener listener)
            : this(new[] { listener })
        {
        }

        public Bus(IReadOnlyList<Listener> listeners)
        {
            this.listeners = new List<Listener>(listeners);
        }

        public void Publish<TMessage>(TMessage message) where TMessage : Message
        {
            foreach (var listener in listeners)
                (listener as Handler<TMessage>)?.Handle(message);
        }
    }
}