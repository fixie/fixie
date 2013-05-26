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
            var cases = convention.CaseMethods(fixtureClass).Select(x => new Case(fixtureClass, x)).ToArray();
            
            foreach (var @case in cases)
            {
                var exceptions = @case.Exceptions;

                object instance;

                if (TryConstruct(fixtureClass, exceptions, out instance))
                {
                    convention.CaseExecutionBehavior.Execute(@case.Method, instance, exceptions);
                    Dispose(instance, exceptions);
                }
            }

            foreach (var @case in cases)
            {
                var exceptions = @case.Exceptions;

                if (exceptions.Any())
                    listener.CaseFailed(@case.Name, exceptions.ToArray());
                else
                    listener.CasePassed(@case.Name);
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