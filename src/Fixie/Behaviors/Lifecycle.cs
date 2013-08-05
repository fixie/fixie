using System;
using System.Reflection;

namespace Fixie.Behaviors
{
    public class Lifecycle
    {
        public static object Construct(Type type)
        {
            try
            {
                return Activator.CreateInstance(type);
            }
            catch (TargetInvocationException ex)
            {
                throw new PreservedException(ex.InnerException);
            }
        }

        public static void Dispose(object instance)
        {
            var disposable = instance as IDisposable;
            if (disposable != null)
                disposable.Dispose();
        }
    }
}