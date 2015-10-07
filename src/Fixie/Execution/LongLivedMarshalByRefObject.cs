using System;
using System.Runtime.Remoting;

namespace Fixie.Execution
{
    /// <summary>
    /// Simplifies the definition of MarshalByRefObject classes whose
    /// instances need to live longer than the default lease lifetime
    /// allows, such as objects passed from a Fixie runner into the
    /// AppDomain of a running test assembly.
    /// 
    /// Instances of LongLivedMarshalByRefObject have an infinite
    /// lease lifetime so that they won't become defective after
    /// several minutes.  As a consequence, instances must be disposed
    /// to free up all resources.
    /// </summary>
    public abstract class LongLivedMarshalByRefObject : MarshalByRefObject, IDisposable
    {
        public override sealed object InitializeLifetimeService()
        {
            // MarshalByRefObjects have lifetimes unlike normal objects.
            // The default implementation of InitializeLifetimeService()
            // causes instances to throw runtime exceptions when the
            // instance lives for several minutes.
            // 
            // This fact poses a problem for long-lived MarshalByRefObjects
            // used in the cross-AppDomain communication between a Fixie
            // runner and the running test assembly. Long-lived tests could
            // cause a MarshalByRefObject Listener, for instance, to become
            // defective when the running test finally finishes.
            // 
            // This class provides a more familiar lifetime for such long-
            // lived MarshalByRefObjects.  Instances claim an infinite
            // lease lifetime by returning null here. To prevent memory leaks
            // as a side effect, Dispose() in order to explicitly end the
            // lifetime.

            return null;
        }

        public virtual void Dispose()
        {
            RemotingServices.Disconnect(this);
        }
    }
}