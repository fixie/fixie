using System.Collections.Generic;
using System.Reflection;
using Fixie.Results;

namespace Fixie.Listeners
{
    public class CompoundListener : Listener
    {
        readonly List<Listener> listeners = new List<Listener>();

        public void Add(Listener listener)
        {
            listeners.Add(listener);
        }

        public void AssemblyStarted(Assembly assembly)
        {
            foreach (var listener in listeners)
                listener.AssemblyStarted(assembly);
        }

        public void CaseSkipped(Case @case)
        {
            foreach (var listener in listeners)
                listener.CaseSkipped(@case);
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