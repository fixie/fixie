using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Fixie
{
    public class MethodCase : Case
    {
        readonly ClassFixture fixture;
        readonly MethodInfo method;
        readonly bool isDeclaredAsync;

        public MethodCase(ClassFixture fixture, MethodInfo method)
        {
            this.fixture = fixture;
            this.method = method;

            isDeclaredAsync = method.GetCustomAttributes<AsyncStateMachineAttribute>(false).Any();
        }

        public string Name
        {
            get { return fixture.Name + "." + method.Name; }
        }

        public void Execute(Listener listener)
        {
            try
            {
                if (isDeclaredAsync && method.ReturnType == typeof(void))
                    ThrowForUnsupportedAsyncVoid();

                object result;
                try
                {
                    result = method.Invoke(fixture.Instance, null);
                }
                catch (TargetInvocationException ex)
                {
                    listener.CaseFailed(this, ex.InnerException);
                    return;
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
                        listener.CaseFailed(this, ex.InnerExceptions.First());
                        return;
                    }
                }

                listener.CasePassed(this);
            }
            catch (Exception ex)
            {
                listener.CaseFailed(this, ex);
            }
        }

        static void ThrowForUnsupportedAsyncVoid()
        {
            throw new NotSupportedException(
                "Async void tests are not supported.  Declare async test methods with " +
                "a return type of Task to ensure the task actually runs to completion.");
        }
    }
}