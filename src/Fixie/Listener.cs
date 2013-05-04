using System;
using System.Reflection;

namespace Fixie
{
    public interface Listener
    {
        void RunStarted(Assembly context);
        void CasePassed(Case @case);
        void CaseFailed(Case @case, Exception[] exceptions);
        void RunComplete(Result result);
    }
}