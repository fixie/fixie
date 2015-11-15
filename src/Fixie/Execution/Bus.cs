using System.Reflection;

namespace Fixie.Execution
{
    public class Bus
    {
        readonly Listener listener;

        public Bus(Listener listener)
        {
            this.listener = listener;
        }

        public void AssemblyStarted(Assembly assembly)
        {
            listener.AssemblyStarted(assembly);
        }

        public void CaseSkipped(SkipResult result)
        {
            listener.CaseSkipped(result);
        }

        public void CasePassed(PassResult result)
        {
            listener.CasePassed(result);
        }

        public void CaseFailed(FailResult result)
        {
            listener.CaseFailed(result);
        }

        public void AssemblyCompleted(Assembly assembly, AssemblyResult result)
        {
            listener.AssemblyCompleted(assembly, result);
        }
    }
}