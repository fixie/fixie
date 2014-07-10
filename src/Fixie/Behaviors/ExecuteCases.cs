using System;

namespace Fixie.Behaviors
{
    public class ExecuteCases : InstanceBehavior
    {
        readonly ExecutionModel executionModel;

        public ExecuteCases(ExecutionModel executionModel)
        {
            this.executionModel = executionModel;
        }

        public void Execute(InstanceExecution instanceExecution, Action next)
        {
            foreach (var caseExecution in instanceExecution.CaseExecutions)
                executionModel.ExecuteCase(instanceExecution.Instance, caseExecution);
        }
    }
}