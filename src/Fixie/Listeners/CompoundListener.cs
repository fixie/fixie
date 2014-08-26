using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fixie.Execution;
using Fixie.Results;

namespace Fixie.Listeners
{
    public class CompoundListener : Listener
    {
        readonly IEnumerable<Listener> listeners;

        public CompoundListener(IEnumerable<Listener> listeners)
        {
            this.listeners = listeners.ToArray();
        }

        public void AssemblyStarted(Assembly assembly)
        {
            foreach (var listener in listeners)
                listener.AssemblyStarted(assembly);
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

        public void AssemblyCompleted(Assembly assembly, AssemblyResult result)
        {
            foreach (var listener in listeners)
                listener.AssemblyCompleted(assembly, result);
        }
    }
}