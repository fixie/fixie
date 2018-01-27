namespace Fixie.Execution
{
    using System;
    using System.Reflection;

    public class AssemblyCompleted : Message
    {
        readonly ExecutionSummary summary;

        public AssemblyCompleted(Assembly assembly, ExecutionSummary summary, TimeSpan duration)
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