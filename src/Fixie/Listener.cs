using System;

namespace Fixie
{
    public interface Listener
    {
        void CaseFailed(Case @case, Exception ex);
    }
}