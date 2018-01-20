namespace Fixie.Execution.Behaviors
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    static class ExecuteCases
    {
        public static void Execute(IReadOnlyList<Case> cases, CaseAction caseLifecycle)
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