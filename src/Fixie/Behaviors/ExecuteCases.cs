using System;
using System.Diagnostics;

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
            {
                using (var console = new RedirectedConsole())
                {
                    var stopwatch = new Stopwatch();
                    stopwatch.Start();

                    try
                    {
                        caseExecution.Instance = instanceExecution.Instance;
                        executionModel.Execute(caseExecution);
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