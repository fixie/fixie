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
                using (var console = new RedirectedConsole())
                {
                    @case.Instance = instanceExecution.Instance;

                    var stopwatch = new Stopwatch();
                    stopwatch.Start();

                    try
                    {
                        caseBehaviors.Execute(@case);
                    }
                    catch (Exception exception)
                    {
                        @case.Fail(exception);
                    }

                    stopwatch.Stop();

                    @case.Instance = null;
                    @case.Execution.Duration = stopwatch.Elapsed;
                    @case.Execution.Output = console.Output;
                }

                Console.Write(@case.Execution.Output);
            }
        }
    }
}