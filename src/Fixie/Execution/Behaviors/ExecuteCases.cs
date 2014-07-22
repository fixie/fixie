using System;
using System.Diagnostics;

namespace Fixie.Execution.Behaviors
{
    public class ExecuteCases : InstanceBehavior
    {
        readonly BehaviorChain<CaseExecution> caseBehaviors;

        public ExecuteCases(BehaviorChain<CaseExecution> caseBehaviors)
        {
            this.caseBehaviors = caseBehaviors;
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
                        caseBehaviors.Execute(caseExecution);
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