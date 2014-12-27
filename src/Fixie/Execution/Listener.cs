namespace Fixie.Execution
{
    public interface Listener
    {
        void AssemblyStarted(AssemblyInfo assembly);
        void CaseSkipped(SkipResult result);
        void CasePassed(PassResult result);
        void CaseFailed(FailResult result);
        void CaseInconclusive(InconclusiveResult result);
        void AssemblyCompleted(AssemblyInfo assembly, AssemblyResult result);
    }
}