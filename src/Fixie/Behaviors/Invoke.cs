using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Fixie.Behaviors
{
    public class Invoke : CaseBehavior
    {
        public void Execute(Case @case, object instance)
        {
            Execute(@case.Method, instance, @case.Exceptions);
        }

        public void Execute(MethodInfo method, object instance, ExceptionList exceptions)
        {
            try
            {
                bool isDeclaredAsync = method.Async();

                if (isDeclaredAsync && method.Void())
                    ThrowForUnsupportedAsyncVoid();

                bool invokeReturned = false;
                object result = null;
                try
                {
                    result = method.Invoke(instance, null);
                    invokeReturned = true;
                }
                catch (TargetInvocationException ex)
                {
                    throw new PreservedException(ex.InnerException);
                }

                if (invokeReturned && isDeclaredAsync)
                {
                    var task = (Task)result;
                    try
                    {
                        task.Wait();
                    }
                    catch (AggregateException ex)
                    {
                        throw new PreservedException(ex.InnerExceptions.First());
                    }
                }
            }
            catch (PreservedException preservedException)
            {
                exceptions.Add(preservedException.OriginalException);
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        }

        static void ThrowForUnsupportedAsyncVoid()
        {
            throw new NotSupportedException(
                "Async void methods are not supported. Declare async methods with a " +
                "return type of Task to ensure the task actually runs to completion.");
        }
    }
}