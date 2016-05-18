namespace Fixie.Internal
{
    using System.Collections.Generic;
    using Execution;

    public class Bus : LongLivedMarshalByRefObject
    {
        readonly List<object> listeners;

        public Bus(object listener)
            : this(new[] { listener })
        {
        }

        public Bus(IReadOnlyCollection<object> listeners)
        {
            this.listeners = new List<object>(listeners);
        }

        public void Publish<TMessage>(TMessage message) where TMessage : Message
        {
            foreach (var listener in listeners)
                (listener as Handler<TMessage>)?.Handle(message);
        }
    }
}