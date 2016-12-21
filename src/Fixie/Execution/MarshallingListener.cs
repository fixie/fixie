namespace Fixie.Execution
{
    public class MarshallingListener : LongLivedMarshalByRefObject, Listener
    {
        readonly Listener listener;

        public MarshallingListener(Listener listener)
        {
            this.listener = listener;
        }

        public void AssemblyStarted(AssemblyStarted message)
            => listener.AssemblyStarted(message);

        public void CaseSkipped(CaseSkipped message)
            => listener.CaseSkipped(message);

        public void CasePassed(CasePassed message)
            => listener.CasePassed(message);

        public void CaseFailed(CaseFailed message)
            => listener.CaseFailed(message);

        public void AssemblyCompleted(AssemblyCompleted message)
            => listener.AssemblyCompleted(message);
    }
}