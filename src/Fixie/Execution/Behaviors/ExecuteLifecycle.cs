namespace Fixie.Execution.Behaviors
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    class ExecuteLifecycle
    {
        readonly Lifecycle lifecycle;

        public ExecuteLifecycle(Lifecycle lifecycle)
        {
            this.lifecycle = lifecycle;
        }

        public void Execute(Type testClass, IReadOnlyList<Case> cases)
        {
            var timeClassExecution = new TimeClassExecution();

            timeClassExecution.Execute(cases, () =>
            {
                try
                {
                    lifecycle.Execute(testClass, caseLifecycle =>
                    {
                        ExecuteCases(caseLifecycle, cases);
                    });
                }
                catch (Exception exception)
                {
                    foreach (var @case in cases)
                        @case.Fail(exception);
                }
            });
        }

        void ExecuteCases(CaseAction caseLifecycle, IReadOnlyList<Case> cases)
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