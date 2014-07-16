using System;

namespace Fixie.Behaviors
{
    public class CreateInstancePerCase : ClassBehavior
    {
        readonly ExecutionPlan executionPlan;

        public CreateInstancePerCase(ExecutionPlan executionPlan)
        {
            this.executionPlan = executionPlan;
        }

        public void Execute(ClassExecution classExecution, Action next)
        {
            foreach (var caseExecution in classExecution.CaseExecutions)
            {
                try
                {
                    executionPlan.PerformClassLifecycle(classExecution.TestClass, new[] { caseExecution });
                }
                catch (Exception exception)
                {
                    caseExecution.Fail(exception);
                }
            }
        }
    }
}