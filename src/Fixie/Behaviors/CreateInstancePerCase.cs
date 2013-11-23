using System;
using Fixie.Conventions;

namespace Fixie.Behaviors
{
    public class CreateInstancePerCase : TypeBehavior
    {
        readonly Func<Type, object> construct;

        public CreateInstancePerCase(Func<Type, object> construct)
        {
            this.construct = construct;
        }

        public void Execute(Type testClass, Convention convention, CaseExecution[] caseExecutions)
        {
            foreach (var caseExecution in caseExecutions)
            {
                try
                {
                    var instance = construct(testClass);

                    var fixture = new Fixture(testClass, instance, convention.CaseExecution.Behavior, new[] { caseExecution });
                    convention.InstanceExecution.Behavior.Execute(fixture);

                    Dispose(instance);
                }
                catch (Exception exception)
                {
                    caseExecution.Fail(exception);
                }
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