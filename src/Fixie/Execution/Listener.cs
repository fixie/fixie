using System.Reflection;

namespace Fixie.Execution
{
    public interface Listener
    {
        void AssemblyStarted(Assembly assembly);
        void CaseSkipped(SkipResult result);
        void CasePassed(PassResult result);
        void CaseFailed(FailResult result);
        void AssemblyCompleted(Assembly assembly, AssemblyResult result);
    }
}