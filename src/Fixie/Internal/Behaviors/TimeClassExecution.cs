namespace Fixie.Internal.Behaviors
{
    using System;
    using System.Diagnostics;
    using System.Linq;

    public class TimeClassExecution : ClassBehavior
    {
        public void Execute(Class context, Action next)
        {
            var stopwatch = Stopwatch.StartNew();
            next();
            stopwatch.Stop();

            var classExecutionDuration = stopwatch.Elapsed;

            var totalCaseDuration = TimeSpan.FromTicks(context.Cases.Sum(x => x.Duration.Ticks));

            // Due to the Stopwatch's precision, it is possible that the sum of multiple
            // imprecise Case measurements will exceed the single imprecise measurement
            // of the whole class's execution. If so, there's no need to adjust timings.

            if (classExecutionDuration > totalCaseDuration)
            {
                var buildChainDuration = classExecutionDuration - totalCaseDuration;

                var numberOfCases = context.Cases.Count;

                var buildChainDurationPerCase = TimeSpan.FromTicks(buildChainDuration.Ticks / numberOfCases);

                foreach (var @case in context.Cases)
                    @case.Duration += buildChainDurationPerCase;
            }
        }
    }
}