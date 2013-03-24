using System;

namespace Fixie
{
    public interface Listener
    {
        void CasePassed(Case @case);
        void CaseFailed(Case @case, Exception ex);
    }
}