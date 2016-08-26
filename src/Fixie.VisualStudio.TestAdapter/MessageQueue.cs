namespace Fixie.VisualStudio.TestAdapter
{
    using System;
    using System.Collections.Concurrent;
    using Runner.Contracts;

    public class MessageQueue : IDisposable
    {
        readonly BlockingCollection<Message> queue;

        public MessageQueue()
        {
            queue = new BlockingCollection<Message>();
        }

        public void Add(Message message) => queue.Add(message);

        public bool TryTake(out Message message)
        {
            message = null;

            try
            {
                //Blocks until there is a message, unless the collection
                //is empty and has been marked as complete for adding.
                message = queue.Take();
            }
            catch (InvalidOperationException)
            {
                return false;
            }

            return true;
        }

        public void Dispose()
        {
            queue.CompleteAdding();

            Message message;
            while (queue.TryTake(out message, millisecondsTimeout: 1)) { }

            queue.Dispose();
        }
    }
}