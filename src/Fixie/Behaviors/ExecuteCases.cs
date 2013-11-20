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
                var result = @case.Result;

                using (var console = new RedirectedConsole())
                {
                    var stopwatch = new Stopwatch();
                    stopwatch.Start();

                    try
                    {
                        fixture.CaseExecutionBehavior.Execute(@case, fixture.Instance);
                    }
                    catch (Exception exception)
                    {
                        @case.Fail(exception);
                    }

                    stopwatch.Stop();

                    result.Duration = stopwatch.Elapsed;
                    result.Output = console.Output;
                }

                Console.Write(result.Output);
            }
        }
    }
}