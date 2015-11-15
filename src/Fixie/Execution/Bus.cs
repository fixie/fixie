using System;
using System.Collections.Generic;
using System.Linq;

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
            foreach (var handler in handlers.OfType<IHandler<TMessage>>()) //TODO: Avoid repeatedly determining listeners by message type.
                handler.Handle(message);
        }
    }
}