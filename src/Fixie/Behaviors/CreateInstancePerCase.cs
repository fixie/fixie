using System;
using System.Linq;
using System.Reflection;
using Fixie.Conventions;

namespace Fixie.Behaviors
{
    public class CreateInstancePerCase : TypeBehavior
    {
        public void Execute(Type fixtureClass, Convention convention, Listener listener)
        {
            var caseMethods = convention.CaseMethods(fixtureClass).ToArray();
            var exceptionsByCase = caseMethods.ToDictionary(x => x, x => new ExceptionList());

            foreach (var caseMethod in caseMethods)
            {
                var exceptions = exceptionsByCase[caseMethod];

                object instance;

                if (TryConstruct(fixtureClass, exceptions, out instance))
                {
                    convention.CaseExecutionBehavior.Execute(caseMethod, instance, exceptions);
                    Dispose(instance, exceptions);
                }
            }

            foreach (var caseMethod in caseMethods)
            {
                var @case = fixtureClass.FullName + "." + caseMethod.Name;
                var exceptions = exceptionsByCase[caseMethod];

                if (exceptions.Any())
                    listener.CaseFailed(@case, exceptions.ToArray());
                else
                    listener.CasePassed(@case);
            }
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