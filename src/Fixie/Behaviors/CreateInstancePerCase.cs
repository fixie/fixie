using System;

namespace Fixie.Behaviors
{
    public class CreateInstancePerCase : ClassBehavior
    {
        public void Execute(ClassExecution classExecution, Action next)
        {
            foreach (var caseExecution in classExecution.CaseExecutions)
            {
                try
                {
                    classExecution.ExecutionModel.PerformClassLifecycle(classExecution, new[] { caseExecution });
                }
                catch (Exception exception)
                {
                    caseExecution.Fail(exception);
                }
            }
        }
    }
}