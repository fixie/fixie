using System;
using System.Reflection;

namespace Fixie
{
    public interface Listener
    {
        void RunStarted(Assembly context);
        void CasePassed(Case @case);
        void CaseFailed(Case @case, Exception ex);
        void RunComplete(Result result);
    }
}