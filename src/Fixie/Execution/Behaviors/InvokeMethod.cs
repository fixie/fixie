using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Fixie.Execution.Behaviors
{
    public class InvokeMethod : CaseBehavior
    {
        public void Execute(Case @case, Action next)
        {
            try
            {
                var caseExecution = @case.Execution;

                var method = @case.Method;

                bool isDeclaredAsync = method.IsAsync();

                if (isDeclaredAsync && method.IsVoid())
                    ThrowForUnsupportedAsyncVoid();

                object result;
                try
                {
                    result = method.Invoke(@case.Instance, @case.Parameters);
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

                    if (method.ReturnType.IsGenericType)
                    {
                        var property = task.GetType().GetProperty("Result", BindingFlags.Instance | BindingFlags.Public);

                        result = property.GetValue(task, null);
                    }
                    else
                    {
                        result = null;
                    }
                }

                caseExecution.Result = result;
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