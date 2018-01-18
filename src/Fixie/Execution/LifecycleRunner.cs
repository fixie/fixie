namespace Fixie.Execution
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    class LifecycleRunner
    {
        readonly Lifecycle lifecycle;

        public LifecycleRunner(Lifecycle lifecycle)
            => this.lifecycle = lifecycle;

        public void Execute(Type testClass, IReadOnlyList<Case> cases)
        {
            var stopwatch = Stopwatch.StartNew();

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

            stopwatch.Stop();

            AdjustCaseDurations(cases, stopwatch);
        }

        static void AdjustCaseDurations(IReadOnlyList<Case> cases, Stopwatch stopwatch)
        {
            var classExecutionDuration = stopwatch.Elapsed;

            var totalCaseDuration = TimeSpan.FromTicks(cases.Sum(x => x.Duration.Ticks));

            // Due to the Stopwatch's precision, it is possible that the sum of multiple
            // imprecise Case measurements will exceed the single imprecise measurement
            // of the whole class's execution. If so, there's no need to adjust timings.

            if (classExecutionDuration > totalCaseDuration)
            {
                var lifecycleOverheadDuration = classExecutionDuration - totalCaseDuration;

                var numberOfCases = cases.Count;

                var lifecycleOverheadDurationPerCase = TimeSpan.FromTicks(lifecycleOverheadDuration.Ticks / numberOfCases);

                foreach (var @case in cases)
                    @case.Duration += lifecycleOverheadDurationPerCase;
            }
        }

        static void ExecuteCases(IReadOnlyList<Case> cases, CaseAction caseLifecycle)
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