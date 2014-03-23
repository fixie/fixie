using System;

namespace Fixie.Behaviors
{
    public class CreateInstancePerTestClass : ClassBehavior
    {
        readonly Func<Type, object> construct;

        public CreateInstancePerTestClass(Func<Type, object> construct)
        {
            this.construct = construct;
        }

        public void Execute(ClassExecution classExecution)
        {
            try
            {
                var instance = construct(classExecution.TestClass);

                var executionPlan = classExecution.ExecutionPlan;
                var instanceExecution = new InstanceExecution(executionPlan, classExecution.TestClass, instance, classExecution.CaseExecutions);
                executionPlan.Execute(instanceExecution);

                Dispose(instance);
            }
            catch (Exception exception)
            {
                classExecution.FailCases(exception);
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