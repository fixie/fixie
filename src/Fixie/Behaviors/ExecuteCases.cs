using System;
using System.Diagnostics;

namespace Fixie.Behaviors
{
    public class ExecuteCases : InstanceBehavior
    {
        public void Execute(Fixture fixture)
        {
            foreach (var @case in fixture.Cases)
            {
                var execution = @case.Execution;

                using (var console = new RedirectedConsole())
                {
                    var stopwatch = new Stopwatch();
                    stopwatch.Start();

                    try
                    {
                        fixture.CaseExecutionBehavior.Execute(@case.Execution, fixture.Instance);
                    }
                    catch (Exception exception)
                    {
                        @case.Fail(exception);
                    }

                    stopwatch.Stop();

                    execution.Duration = stopwatch.Elapsed;
                    execution.Output = console.Output;
                }

                Console.Write(execution.Output);
            }
        }
    }
}