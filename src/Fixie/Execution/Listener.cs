using Fixie.Results;

namespace Fixie.Execution
{
    public interface Listener
    {
        void AssemblyStarted(string assemblyFileName);
        void CaseSkipped(SkipResult result);
        void CasePassed(PassResult result);
        void CaseFailed(FailResult result);
        void AssemblyCompleted(string assemblyFileName, AssemblyResult result);
    }
}