using Fixie.Execution;
using System.Collections.Generic;

namespace Fixie.Internal
{
    public class Bus : LongLivedMarshalByRefObject
    {
        readonly List<object> listeners;

        public Bus(params object[] listeners)
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