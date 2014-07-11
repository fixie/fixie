using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Fixie.Behaviors
{
    public class InvokeMethod : CaseBehavior
    {
        public void Execute(CaseExecution caseExecution, Action next)
        {
            try
            {
                var @case = caseExecution.Case;
                var method = @case.Method;

                bool isDeclaredAsync = method.IsAsync();

                if (isDeclaredAsync && method.IsVoid())
                    ThrowForUnsupportedAsyncVoid();

                object result;
                try
                {
                    result = method.Invoke(caseExecution.Instance, @case.Parameters);
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
                caseExecution.Fail(exception);
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