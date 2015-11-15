namespace Fixie.Execution
{
    public interface Listener
    {
        void AssemblyStarted(AssemblyStarted message);
        void CaseSkipped(SkipResult result);
        void CasePassed(PassResult result);
        void CaseFailed(FailResult result);
        void AssemblyCompleted(AssemblyCompleted message);
    }
}