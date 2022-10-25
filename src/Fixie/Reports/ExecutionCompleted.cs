namespace Fixie.Reports
{
    using System;
    using Internal;

    /// <summary>
    /// Fired once at the end of the execution phase for the test assembly.
    /// </summary>
    public class ExecutionCompleted : IMessage
    {
        readonly ExecutionSummary summary;

        internal ExecutionCompleted(ExecutionSummary summary, TimeSpan duration)
        {
            Duration = duration;
            this.summary = summary;
        }

        /// <summary>
        /// The count of passing test results for the test assembly.
        /// </summary>
        public int Passed => summary.Passed;
        
        /// <summary>
        /// The count of failing test results for the test assembly.
        /// </summary>
        public int Failed => summary.Failed;
        
        /// <summary>
        /// The count of skipped test results for the test assembly.
        /// </summary>
        public int Skipped => summary.Skipped;
        
        /// <summary>
        /// The total count of all test results for the test assembly.
        /// </summary>
        public int Total => summary.Total;
        
        /// <summary>
        /// The total duration of the test assembly execution.
        /// </summary>
        public TimeSpan Duration { get; }
    }
}