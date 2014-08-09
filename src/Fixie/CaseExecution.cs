using System;
using System.Collections.Generic;

namespace Fixie
{
    public class CaseExecution
    {
        readonly List<Exception> exceptions;

        public CaseExecution()
        {
            exceptions = new List<Exception>();
        }
        
        public IReadOnlyList<Exception> Exceptions { get { return exceptions; } }

        public void Fail(Exception reason)
        {
            var wrapped = reason as PreservedException;

            if (wrapped != null)
                exceptions.Add(wrapped.OriginalException);
            else
                exceptions.Add(reason);
        }

        public void ClearExceptions()
        {
            exceptions.Clear();
        }
    }
}