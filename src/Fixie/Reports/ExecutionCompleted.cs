namespace Fixie.Reports
{
    using System;
    using System.Reflection;
    using Internal;

    public class ExecutionCompleted : IMessage
    {
        readonly ExecutionSummary summary;

        internal ExecutionCompleted(ExecutionSummary summary, TimeSpan duration)
        {
            Duration = duration;
            this.summary = summary;
        }

        public int Passed => summary.Passed;
        public int Failed => summary.Failed;
        public int Skipped => summary.Skipped;
        public int Total => summary.Total;
        public TimeSpan Duration { get; }
    }
}