namespace Fixie.Internal
{
    using System;
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
            {
                try
                {
                    (listener as Handler<TMessage>)?.Handle(message);
                    (listener as AsyncHandler<TMessage>)?.Handle(message).GetAwaiter().GetResult();
                }
                catch (Exception exception)
                {
                    using (Foreground.Yellow)
                        Console.WriteLine(
                            $"{listener.GetType().FullName} threw an exception while " +
                            $"attempting to handle a message of type {typeof(TMessage).FullName}:");
                    Console.WriteLine();
                    Console.WriteLine(exception.ToString());
                    Console.WriteLine();
                }
            }
        }
    }
}