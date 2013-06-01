using System;
using System.Reflection;
using Fixie.Conventions;

namespace Fixie.Behaviors
{
    public class CreateInstancePerFixture : TypeBehavior
    {
        public void Execute(Type fixtureClass, Convention convention, Case[] cases)
        {
            object instance;
            var constructionExceptions = new ExceptionList();
            if (!TryConstruct(fixtureClass, constructionExceptions, out instance))
            {
                foreach (var @case in cases)
                    @case.Exceptions.Add(constructionExceptions);
            }
            else
            {
                convention.InstanceExecution.Behavior.Execute(fixtureClass, instance, cases, convention);

                var disposalExceptions = Dispose(instance);
                if (disposalExceptions.Any())
                {
                    foreach (var @case in cases)
                        @case.Exceptions.Add(disposalExceptions);
                }
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

        static ExceptionList Dispose(object instance)
        {
            var exceptions = new ExceptionList();

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

            return exceptions;
        }
    }
}