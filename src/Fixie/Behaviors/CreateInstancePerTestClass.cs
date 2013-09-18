using System;
using Fixie.Conventions;

namespace Fixie.Behaviors
{
    public class CreateInstancePerTestClass : TypeBehavior
    {
        readonly Func<Type, object> construct;

        public CreateInstancePerTestClass(Func<Type, object> construct)
        {
            this.construct = construct;
        }

        public void Execute(Type testClass, Convention convention, Case[] cases)
        {
            try
            {
                var instance = construct(testClass);

                var fixture = new Fixture(testClass, instance, convention.CaseExecution.Behavior, new InvokeMethod(), cases);
                convention.InstanceExecution.Behavior.Execute(fixture);

                Dispose(instance);
            }
            catch (Exception exception)
            {
                foreach (var @case in cases)
                    @case.Fail(exception);
            }
        }

        static void Dispose(object instance)
        {
            var disposable = instance as IDisposable;
            if (disposable != null)
                disposable.Dispose();
        }
    }
}