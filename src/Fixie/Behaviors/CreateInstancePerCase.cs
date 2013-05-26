using System;
using System.Reflection;
using Fixie.Conventions;

namespace Fixie.Behaviors
{
    public class CreateInstancePerCase : TypeBehavior
    {
        public void Execute(Type fixtureClass, Convention convention, Case[] cases)
        {
            foreach (var @case in cases)
            {
                var exceptions = @case.Exceptions;

                object instance;

                if (TryConstruct(fixtureClass, exceptions, out instance))
                {
                    convention.InstanceExecutionBehavior.Execute(fixtureClass, instance, new[] { @case }, convention);

                    Dispose(instance, exceptions);
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