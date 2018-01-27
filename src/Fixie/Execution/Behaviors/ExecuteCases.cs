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
                string consoleOutput;
                using (var console = new RedirectedConsole())
                {
                    var stopwatch = Stopwatch.StartNew();

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

                    consoleOutput = console.Output;
                    @case.Output += consoleOutput;
                }

                Console.Write(consoleOutput);
            }
        }
    }
}