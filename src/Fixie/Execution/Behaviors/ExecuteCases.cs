using System;
using System.Diagnostics;

namespace Fixie.Execution.Behaviors
{
    public class ExecuteCases : InstanceBehavior
    {
        readonly BehaviorChain<Case> caseBehaviors;

        public ExecuteCases(BehaviorChain<Case> caseBehaviors)
        {
            this.caseBehaviors = caseBehaviors;
        }

        public void Execute(InstanceExecution instanceExecution, Action next)
        {
            foreach (var @case in instanceExecution.Cases)
            {
                var caseExecution = @case.Execution;
                using (var console = new RedirectedConsole())
                {
                    caseExecution.Instance = instanceExecution.Instance;

                    var stopwatch = new Stopwatch();
                    stopwatch.Start();

                    try
                    {
                        caseBehaviors.Execute(caseExecution.Case);
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