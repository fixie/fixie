namespace Fixie.Execution.Behaviors
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    class ExecuteCases
    {
        public void Execute(CaseAction caseLifecycle, IReadOnlyList<Case> cases)
        {
            foreach (var @case in cases)
            {
                using (var console = new RedirectedConsole())
                {
                    var stopwatch = new Stopwatch();
                    stopwatch.Start();

                    try
                    {
                        caseLifecycle(@case);
                    }
                    catch (Exception exception)
                    {
                        @case.Fail(exception);
                    }

                    stopwatch.Stop();

                    @case.Duration += stopwatch.Elapsed;
                    @case.Output = console.Output;
                }

                Console.Write(@case.Output);
            }
        }
    }
}