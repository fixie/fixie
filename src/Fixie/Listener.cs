using System.Reflection;

namespace Fixie
{
    public interface Listener
    {
        void AssemblyStarted(Assembly assembly);
        void CasePassed(PassResult result);
        void CaseFailed(FailResult result);
        void AssemblyCompleted(Assembly assembly, Result result);
    }
}