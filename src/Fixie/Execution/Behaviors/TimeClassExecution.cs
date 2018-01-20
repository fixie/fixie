namespace Fixie.Execution.Behaviors
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    static class TimeClassExecution
    {
        public static void Execute(IReadOnlyList<Case> cases, Action next)
        {
            var stopwatch = Stopwatch.StartNew();
            next();
            stopwatch.Stop();

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
    }
}