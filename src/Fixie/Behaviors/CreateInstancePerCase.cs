using System;

namespace Fixie.Behaviors
{
    public class CreateInstancePerCase : TypeBehavior
    {
        readonly Func<Type, object> construct;

        public CreateInstancePerCase(Func<Type, object> construct)
        {
            this.construct = construct;
        }

        public void Execute(ClassExecution classExecution)
        {
            foreach (var caseExecution in classExecution.CaseExecutions)
            {
                try
                {
                    var instance = construct(classExecution.TestClass);

                    var instanceExecution = new InstanceExecution(classExecution.Convention, classExecution.TestClass, instance, new[] { caseExecution });
                    classExecution.Convention.InstanceExecution.Behavior.Execute(instanceExecution);

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