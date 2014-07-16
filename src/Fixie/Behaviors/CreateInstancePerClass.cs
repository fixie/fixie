using System;

namespace Fixie.Behaviors
{
    public class CreateInstancePerClass : ClassBehavior
    {
        readonly ExecutionPlan executionPlan;

        public CreateInstancePerClass(ExecutionPlan executionPlan)
        {
            this.executionPlan = executionPlan;
        }

        public void Execute(ClassExecution classExecution, Action next)
        {
            try
            {
                executionPlan.PerformClassLifecycle(classExecution.TestClass, classExecution.CaseExecutions);
            }
            catch (Exception exception)
            {
                classExecution.Fail(exception);
            }
        }
    }
}