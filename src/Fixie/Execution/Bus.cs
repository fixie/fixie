using System.Collections.Generic;

namespace Fixie.Execution
{
    public class Bus
    {
        readonly List<object> subscribers = new List<object>();

        public void Subscribe(object subscriber)
        {
            subscribers.Add(subscriber);
        }

        public void Publish<TMessage>(TMessage message) where TMessage : IMessage
        {
            foreach (var subscriber in subscribers)
                (subscriber as IHandler<TMessage>)?.Handle(message);
        }
    }
}