namespace Fixie.Execution.Listeners
{
    using System;
    using System.Collections.Generic;

    public class ClassReport
    {
        readonly List<CaseCompleted> cases;
        readonly ExecutionSummary summary;

        public ClassReport(Type @class)
        {
            cases = new List<CaseCompleted>();
            summary = new ExecutionSummary();
            Class = @class;
        }

        public void Add(CaseCompleted message)
        {
            cases.Add(message);
            summary.Add(message);
        }

        public Type Class { get; }

        public IReadOnlyList<CaseCompleted> Cases => cases;

        public int Passed => summary.Passed;
        public int Failed => summary.Failed;
        public int Skipped => summary.Skipped;
        public TimeSpan Duration => summary.Duration;
    }
}