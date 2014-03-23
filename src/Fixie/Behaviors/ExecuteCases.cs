using System;
using System.Diagnostics;

namespace Fixie.Behaviors
{
    public class ExecuteCases : InstanceBehavior
    {
        public void Execute(InstanceExecution instanceExecution)
        {
            foreach (var caseExecution in instanceExecution.CaseExecutions)
            {

                using (var console = new RedirectedConsole())
                {
                    var stopwatch = new Stopwatch();
                    stopwatch.Start();

                    try
                    {
                        var executionPlan = instanceExecution.ExecutionPlan;
                        var instance = instanceExecution.Instance;
                        executionPlan.Execute(caseExecution, instance);
                    }
                    catch (Exception exception)
                    {
                        caseExecution.Fail(exception);
                    }

                    stopwatch.Stop();

                    caseExecution.Duration = stopwatch.Elapsed;
                    caseExecution.Output = console.Output;
                }

                Console.Write(caseExecution.Output);
            }
        }
    }
}