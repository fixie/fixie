using System;

namespace Fixie.Behaviors
{
    public class CreateInstancePerClass : ClassBehavior
    {
        public void Execute(ClassExecution classExecution, Action next)
        {
            try
            {
                classExecution.ExecutionModel.PerformClassLifecycle(classExecution, classExecution.CaseExecutions);
            }
            catch (Exception exception)
            {
                classExecution.Fail(exception);
            }
        }
    }
}