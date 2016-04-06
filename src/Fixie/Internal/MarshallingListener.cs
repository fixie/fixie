using Fixie.Execution;

namespace Fixie.Internal
{
    public class MarshallingListener : LongLivedMarshalByRefObject, Listener
    {
        readonly Listener listener;

        public MarshallingListener(Listener listener) { this.listener = listener; }

        public void AssemblyStarted(AssemblyInfo assembly) => listener.AssemblyStarted(assembly);
        public void CaseSkipped(SkipResult result) => listener.CaseSkipped(result);
        public void CasePassed(PassResult result) => listener.CasePassed(result);
        public void CaseFailed(FailResult result) => listener.CaseFailed(result);
        public void AssemblyCompleted(AssemblyCompleted message) => listener.AssemblyCompleted(message);
    }
}