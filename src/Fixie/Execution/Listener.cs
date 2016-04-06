namespace Fixie.Execution
{
    public interface Listener
    {
        void Handle(AssemblyInfo assembly);
        void Handle(SkipResult result);
        void Handle(PassResult result);
        void Handle(FailResult result);
        void Handle(AssemblyCompleted message);
    }
}