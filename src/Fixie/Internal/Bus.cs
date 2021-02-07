namespace Fixie.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Reports;

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

        public async Task PublishAsync<TMessage>(TMessage message) where TMessage : Message
        {
            foreach (var listener in listeners)
            {
                try
                {
                    if (listener is Handler<TMessage> handler)
                        handler.Handle(message);

                    if (listener is AsyncHandler<TMessage> asyncHandler)
                        await asyncHandler.HandleAsync(message);
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