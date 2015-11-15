using System.Collections.Generic;

namespace Fixie.Execution
{
    public class Bus
    {
        readonly List<object> handlers = new List<object>();

        public void Subscribe(object handler)
        {
            handlers.Add(handler);
        }

        public void Publish<TMessage>(TMessage message) where TMessage : IMessage
        {
            foreach (var handler in handlers)
                (handler as IHandler<TMessage>)?.Handle(message);
        }
    }
}