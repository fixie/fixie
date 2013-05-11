using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Fixie
{
    public class MethodCase : Case
    {
        readonly ClassFixture fixture;
        readonly MethodInfo method;

        public MethodCase(ClassFixture fixture, MethodInfo method)
        {
            this.fixture = fixture;
            this.method = method;
        }

        public string Name
        {
            get { return fixture.Name + "." + method.Name; }
        }

        public void Execute(Listener listener, List<Exception> exceptions)
        {
            bool isDeclaredAsync = method.Async();

            if (isDeclaredAsync && method.Void())
                ThrowForUnsupportedAsyncVoid();

            bool invokeReturned = false;
            object result = null;
            try
            {
                result = method.Invoke(fixture.Instance, null);
                invokeReturned = true;
            }
            catch (TargetInvocationException ex)
            {
                exceptions.Add(ex.InnerException);
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
                    exceptions.Add(ex.InnerExceptions.First());
                }
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