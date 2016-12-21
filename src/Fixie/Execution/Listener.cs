namespace Fixie.Execution
{
    public interface Listener
    {
        void AssemblyStarted(AssemblyStarted message);
        void CaseSkipped(CaseSkipped message);
        void CasePassed(CasePassed message);
        void CaseFailed(CaseFailed message);
        void AssemblyCompleted(AssemblyCompleted message);
    }
}