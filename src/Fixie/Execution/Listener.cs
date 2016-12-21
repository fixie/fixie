namespace Fixie.Execution
{
    public interface Listener
    {
        void Handle(AssemblyStarted message);
        void Handle(CaseSkipped message);
        void Handle(CasePassed message);
        void Handle(CaseFailed message);
        void Handle(AssemblyCompleted message);
    }
}