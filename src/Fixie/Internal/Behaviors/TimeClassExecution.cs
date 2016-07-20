namespace Fixie.Internal.Behaviors
{
    using System;
    using System.Diagnostics;
    using System.Linq;

    public class TimeClassExecution : ClassBehavior
    {
        public void Execute(Class context, Action next)
        {
            var sw = Stopwatch.StartNew();
            next();
            sw.Stop();
            var classExecutionDuration = sw.Elapsed;

            var totalCaseDuration = TimeSpan.FromTicks(context.Cases.Sum(x => x.Duration.Ticks));

            var numberOfCases = context.Cases.Count;

            var buildChainDuration = classExecutionDuration - totalCaseDuration;

            var buildChainDurationPerCase = TimeSpan.FromTicks(buildChainDuration.Ticks/numberOfCases);

            foreach (var @case in context.Cases)
            {
                @case.Duration = @case.Duration + buildChainDurationPerCase;
            }
        }
    }
}