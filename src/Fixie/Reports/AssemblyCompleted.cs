namespace Fixie.Reports
{
    using System;
    using System.Reflection;
    using Internal;

    public class AssemblyCompleted : IMessage
    {
        readonly ExecutionSummary summary;

        internal AssemblyCompleted(Assembly assembly, ExecutionSummary summary, TimeSpan duration)
        {
            Assembly = assembly;
            Duration = duration;
            this.summary = summary;
        }

        public Assembly Assembly { get; }
        public int Passed => summary.Passed;
        public int Failed => summary.Failed;
        public int Skipped => summary.Skipped;
        public int Total => summary.Total;
        public TimeSpan Duration { get; }
    }
}