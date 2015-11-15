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

        public void Publish(AssemblyStarted message)
        {
            foreach (var listener in listeners)
                listener.Handle(message);
        }

        public void Publish(SkipResult result)
        {
            foreach (var listener in listeners)
                listener.Handle(result);
        }

        public void Publish(PassResult result)
        {
            foreach (var listener in listeners)
                listener.Handle(result);
        }

        public void Publish(FailResult result)
        {
            foreach (var listener in listeners)
                listener.Handle(result);
        }

        public void Publish(AssemblyCompleted message)
        {
            foreach (var listener in listeners)
                listener.Handle(message);
        }
    }
}