using System.Collections.Generic;

namespace Fixie.Execution
{
    public class Bus
    {
        readonly List<Listener> listeners = new List<Listener>();

        public void Subscribe(Listener listener)
        {
            listeners.Add(listener);
        }

        public void Publish<TMessage>(TMessage message) where TMessage : IMessage
        {
            foreach (var listener in listeners)
            {
                var handler = (IHandler<TMessage>)listener;
                handler.Handle(message);
            }
        }
    }
}