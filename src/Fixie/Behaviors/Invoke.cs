using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Fixie.Behaviors
{
    public class Invoke : CaseBehavior
    {
        public void Execute(InvokeBehavior invokeBehavior, Case @case, object instance)
        {
            try
            {
                var method = @case.Method;

                bool isDeclaredAsync = method.IsAsync();

                if (isDeclaredAsync && method.IsVoid())
                    ThrowForUnsupportedAsyncVoid();

                object result;
                try
                {
                    result = invokeBehavior.Execute(method, instance);
                }
                catch (TargetInvocationException exception)
                {
                    throw new PreservedException(exception.InnerException);
                }

                if (isDeclaredAsync)
                {
                    var task = (Task)result;
                    try
                    {
                        task.Wait();
                    }
                    catch (AggregateException exception)
                    {
                        throw new PreservedException(exception.InnerExceptions.First());
                    }
                }
            }
            catch (Exception exception)
            {
                @case.Fail(exception);
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