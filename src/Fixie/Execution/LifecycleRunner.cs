namespace Fixie.Execution
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    class LifecycleRunner
    {
        readonly Lifecycle lifecycle;

        public LifecycleRunner(Lifecycle lifecycle)
            => this.lifecycle = lifecycle;

        public void Execute(Type testClass, IReadOnlyList<Case> cases)
        {
            try
            {
                lifecycle.Execute(testClass, caseLifecycle =>
                {
                    ExecuteCases(cases, caseLifecycle);
                });
            }
            catch (Exception exception)
            {
                foreach (var @case in cases)
                    @case.Fail(exception);
            }
        }

        static void ExecuteCases(IReadOnlyList<Case> cases, CaseAction caseLifecycle)
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