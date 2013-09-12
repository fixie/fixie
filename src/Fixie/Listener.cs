using System;
using System.Reflection;

namespace Fixie
{
    public interface Listener
    {
        void AssemblyStarted(Assembly assembly);
        void CasePassed(Case @case);
        void CaseFailed(Case @case, Exception[] exceptions);
        void AssemblyCompleted(Assembly assembly, Result result);
    }
}