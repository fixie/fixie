using System;
using System.Reflection;

namespace Fixie
{
    public interface Listener
    {
        void RunStarted(Assembly context);
        void CasePassed(string @case);
        void CaseFailed(string @case, Exception[] exceptions);
        void RunComplete(Result result);
    }
}