namespace Fixie.Execution
{
    public interface Listener
    {
        void AssemblyStarted(AssemblyInfo message);
        void CaseSkipped(CaseSkipped message);
        void CasePassed(CasePassed message);
        void CaseFailed(CaseFailed message);
        void AssemblyCompleted(AssemblyInfo message, AssemblyResult result);
    }
}