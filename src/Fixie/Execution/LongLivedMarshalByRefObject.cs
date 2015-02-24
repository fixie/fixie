using System;
using System.Runtime.Remoting;

namespace Fixie.Execution
{
    /// <summary>
    /// Normally, inheriting from MarshalByRefObject introduces an unexpected defect
    /// in which your instances cease to exist after they get a few minutes old.
    /// 
    /// For instance, when a runner provides a MarshalByRefObject to act as the test
    /// Listener, that Listener may disappear after a long-running test finally finishes,
    /// causing a runtime exception that interrupts the run. The Listener would no longer
    /// be available to receive the test's results.
    /// 
    /// Inheriting from this alternative to MarshalByRefObject allows the instance
    /// to be kept alive indefinitely, but you must Dispose() of it yourself when
    /// its natural lifetime has been reached.
    /// </summary>
    public class LongLivedMarshalByRefObject : MarshalByRefObject, IDisposable
    {
        public override sealed object InitializeLifetimeService()
        {
            //Returning null here causes the instance to live indefinitely.

            //A consequence of keeping a MarshalByRefObject alive like this is that
            //it must be explicitly cleaned up with a call to RemotingServices.Disconnect(this).
            //See Dispose().

            return null;
        }

        public void Dispose()
        {
            RemotingServices.Disconnect(this);
        }
    }
}