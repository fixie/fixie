using System;

namespace Fixie.Behaviors
{
    public class CreateInstancePerClass : ClassBehavior
    {
        readonly Func<Type, object> construct;

        public CreateInstancePerClass(Func<Type, object> construct)
        {
            this.construct = construct;
        }

        public void Execute(ClassExecution classExecution, Action next)
        {
            try
            {
                var instance = construct(classExecution.TestClass);

                classExecution.ExecutionModel.Execute(classExecution, instance, classExecution.CaseExecutions);

                Dispose(instance);
            }
            catch (Exception exception)
            {
                classExecution.Fail(exception);
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