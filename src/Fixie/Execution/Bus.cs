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

        public void AssemblyStarted(AssemblyStarted message)
        {
            foreach (var listener in listeners)
                listener.AssemblyStarted(message);
        }

        public void CaseSkipped(SkipResult result)
        {
            foreach (var listener in listeners)
                listener.CaseSkipped(result);
        }

        public void CasePassed(PassResult result)
        {
            foreach (var listener in listeners)
                listener.CasePassed(result);
        }

        public void CaseFailed(FailResult result)
        {
            foreach (var listener in listeners)
                listener.CaseFailed(result);
        }

        public void AssemblyCompleted(AssemblyCompleted message)
        {
            foreach (var listener in listeners)
                listener.AssemblyCompleted(message);
        }
    }
}