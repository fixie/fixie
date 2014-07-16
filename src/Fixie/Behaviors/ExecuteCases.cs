using System;
using System.Diagnostics;

namespace Fixie.Behaviors
{
    public class ExecuteCases : InstanceBehavior
    {
        readonly ExecutionPlan executionPlan;

        public ExecuteCases(ExecutionPlan executionPlan)
        {
            this.executionPlan = executionPlan;
        }

        public void Execute(InstanceExecution instanceExecution, Action next)
        {
            foreach (var caseExecution in instanceExecution.CaseExecutions)
            {
                using (var console = new RedirectedConsole())
                {
                    caseExecution.Instance = instanceExecution.Instance;

                    var stopwatch = new Stopwatch();
                    stopwatch.Start();

                    try
                    {
                        executionPlan.ExecuteCaseBehaviors(caseExecution);
                    }
                    catch (Exception exception)
                    {
                        caseExecution.Fail(exception);
                    }

                    stopwatch.Stop();

                    caseExecution.Instance = null;
                    caseExecution.Duration = stopwatch.Elapsed;
                    caseExecution.Output = console.Output;
                }

                Console.Write(caseExecution.Output);
            }
        }
    }
}