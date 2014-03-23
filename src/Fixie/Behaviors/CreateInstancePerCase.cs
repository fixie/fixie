using System;

namespace Fixie.Behaviors
{
    public class CreateInstancePerCase : ClassBehavior
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

                    var executionPlan = classExecution.ExecutionPlan;
                    var instanceExecution = new InstanceExecution(executionPlan, classExecution.TestClass, instance, new[] { caseExecution });
                    executionPlan.Execute(instanceExecution);

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