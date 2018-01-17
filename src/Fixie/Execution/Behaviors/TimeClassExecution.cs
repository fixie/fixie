namespace Fixie.Execution.Behaviors
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    class TimeClassExecution
    {
        public void Execute(IReadOnlyList<Case> cases, Action next)
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
                var buildChainDuration = classExecutionDuration - totalCaseDuration;

                var numberOfCases = cases.Count;

                var buildChainDurationPerCase = TimeSpan.FromTicks(buildChainDuration.Ticks / numberOfCases);

                foreach (var @case in cases)
                    @case.Duration += buildChainDurationPerCase;
            }
        }
    }
}