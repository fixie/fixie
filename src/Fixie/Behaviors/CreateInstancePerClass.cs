using System;

namespace Fixie.Behaviors
{
    public class CreateInstancePerClass : ClassBehavior
    {
        readonly ExecutionModel executionModel;

        public CreateInstancePerClass(ExecutionModel executionModel)
        {
            this.executionModel = executionModel;
        }

        public void Execute(ClassExecution classExecution, Action next)
        {
            try
            {
                executionModel.PerformClassLifecycle(classExecution, classExecution.CaseExecutions);
            }
            catch (Exception exception)
            {
                classExecution.Fail(exception);
            }
        }
    }
}