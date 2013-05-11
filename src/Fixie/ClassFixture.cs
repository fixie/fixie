using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Fixie
{
    public class ClassFixture : Fixture
    {
        readonly Type fixtureClass;
        readonly Convention convention;

        public ClassFixture(Type fixtureClass, Convention convention)
        {
            this.fixtureClass = fixtureClass;
            this.convention = convention;
        }

        public string Name
        {
            get { return fixtureClass.FullName; }
        }

        public void Execute(Listener listener)
        {
            foreach (var caseMethod in convention.CaseMethods(fixtureClass))
                Lifecycle(caseMethod, listener);
        }

        void Lifecycle(MethodInfo caseMethod, Listener listener)
        {
            var @case = new MethodCase(this, caseMethod);

            var exceptions = new ExceptionList();

            object instance;

            if (TryConstruct(fixtureClass, exceptions, out instance))
            {
                ExecuteCaseMethod(caseMethod, instance, exceptions);
                Dispose(instance, exceptions);
            }

            if (exceptions.Any())
                listener.CaseFailed(@case, exceptions.ToArray());
            else
                listener.CasePassed(@case);
        }

        static bool TryConstruct(Type fixtureClass, ExceptionList exceptions, out object instance)
        {
            try
            {
                instance = Activator.CreateInstance(fixtureClass);
                return true;
            }
            catch (TargetInvocationException ex)
            {
                exceptions.Add(ex.InnerException);
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }

            instance = null;
            return false;
        }

        static void ExecuteCaseMethod(MethodInfo caseMethod, object fixtureInstance, ExceptionList exceptions)
        {
            try
            {
                bool isDeclaredAsync = caseMethod.Async();

                if (isDeclaredAsync && caseMethod.Void())
                    ThrowForUnsupportedAsyncVoid();

                bool invokeReturned = false;
                object result = null;
                try
                {
                    result = caseMethod.Invoke(fixtureInstance, null);
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
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        }

        static void Dispose(object instance, ExceptionList exceptions)
        {
            try
            {
                var disposable = instance as IDisposable;
                if (disposable != null)
                    disposable.Dispose();
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
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