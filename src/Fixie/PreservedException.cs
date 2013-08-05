using System;

namespace Fixie
{
    public class PreservedException : Exception
    {
        public PreservedException(Exception originalException)
        {
            OriginalException = originalException;
        }

        public Exception OriginalException { get; private set; }
    }
}
