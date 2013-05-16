using System;
using System.Reflection;

namespace Fixie
{
    public class ClassFixture : Fixture
    {
        readonly Type fixtureClass;
        readonly Convention convention;
        readonly MethodBehavior caseMethodBehavior;

        public ClassFixture(Type fixtureClass, Convention convention)
        {
            this.fixtureClass = fixtureClass;
            this.convention = convention;

            caseMethodBehavior = new Invoke();
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
                caseMethodBehavior.Execute(caseMethod, instance, exceptions);
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
    }
}