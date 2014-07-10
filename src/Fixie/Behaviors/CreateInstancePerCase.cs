using System;

namespace Fixie.Behaviors
{
    public class CreateInstancePerCase : ClassBehavior
    {
        readonly ExecutionModel executionModel;

        public CreateInstancePerCase(ExecutionModel executionModel)
        {
            this.executionModel = executionModel;
        }

        public void Execute(ClassExecution classExecution, Action next)
        {
            foreach (var caseExecution in classExecution.CaseExecutions)
            {
                try
                {
                    executionModel.PerformClassLifecycle(classExecution.TestClass, new[] { caseExecution });
                }
                catch (Exception exception)
                {
                    caseExecution.Fail(exception);
                }
            }
        }
    }
}