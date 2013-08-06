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
            try
            {
                var method = @case.Method;

                bool isDeclaredAsync = method.Async();

                if (isDeclaredAsync && method.Void())
                    ThrowForUnsupportedAsyncVoid();

                object result;
                try
                {
                    result = method.Invoke(instance, null);
                }
                catch (TargetInvocationException ex)
                {
                    throw new PreservedException(ex.InnerException);
                }

                if (isDeclaredAsync)
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
                @case.Exceptions.Add(preservedException.OriginalException);
            }
            catch (Exception ex)
            {
                @case.Exceptions.Add(ex);
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