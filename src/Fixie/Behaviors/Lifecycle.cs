using System;
using System.Reflection;

namespace Fixie.Behaviors
{
    public class Lifecycle
    {
        public static ExceptionList Construct(Type type, out object instance)
        {
            var exceptions = new ExceptionList();
            
            instance = null;

            try
            {
                instance = Activator.CreateInstance(type);
            }
            catch (TargetInvocationException ex)
            {
                exceptions.Add(ex.InnerException);
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }

            return exceptions;
        }

        public static void Dispose(object instance)
        {
            var disposable = instance as IDisposable;
            if (disposable != null)
                disposable.Dispose();
        }
    }
}