namespace Fixie.Internal
{
    using System;

    public class ClassCompleted : Message
    {
        readonly ExecutionSummary summary;

        public ClassCompleted(Type @class, ExecutionSummary summary, TimeSpan duration)
        {
            Class = @class;
            Duration = duration;
            this.summary = summary;
        }

        public Type Class { get; }
        public int Passed => summary.Passed;
        public int Failed => summary.Failed;
        public int Skipped => summary.Skipped;
        public int Total => summary.Total;
        public TimeSpan Duration { get; }
    }
}