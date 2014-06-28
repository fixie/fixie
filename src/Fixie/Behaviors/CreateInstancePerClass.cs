using System;
using System.Collections.Generic;

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
                PerformClassLifecycle(classExecution, classExecution.CaseExecutions);
            }
            catch (Exception exception)
            {
                classExecution.Fail(exception);
            }
        }

        void PerformClassLifecycle(ClassExecution classExecution, IReadOnlyList<CaseExecution> caseExecutionsForThisInstance)
        {
            var instance = construct(classExecution.TestClass);

            classExecution.ExecutionModel.Execute(classExecution, instance, caseExecutionsForThisInstance);

            Dispose(instance);
        }

        static void Dispose(object instance)
        {
            var disposable = instance as IDisposable;
            if (disposable != null)
                disposable.Dispose();
        }
    }
}