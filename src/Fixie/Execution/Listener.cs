namespace Fixie.Execution
{
    public interface Listener
    {
        void AssemblyStarted(AssemblyInfo message);
        void CaseSkipped(SkipResult message);
        void CasePassed(PassResult message);
        void CaseFailed(FailResult message);
        void AssemblyCompleted(AssemblyInfo message, AssemblyResult result);
    }
}