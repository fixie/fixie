using System;
using System.Reflection;

namespace Fixie
{
    public interface Listener
    {
        void AssemblyStarted(Assembly assembly);
        void CasePassed(string @case);
        void CaseFailed(string @case, Exception[] exceptions);
        void AssemblyCompleted(Assembly assembly, Result result);
    }
}