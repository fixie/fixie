namespace Fixie.Execution.Listeners
{
    using System;
    using System.Collections.Generic;
    using Execution;

    public class ClassReport
    {
        readonly List<CaseCompleted> cases;
        readonly ExecutionSummary summary;

        public ClassReport(Type testClass)
        {
            TestClass = testClass;
            cases = new List<CaseCompleted>();
            summary = new ExecutionSummary();
        }

        public void Add(CaseCompleted message)
        {
            cases.Add(message);
            summary.Add(message);
        }

        public Type TestClass { get; }

        public IReadOnlyList<CaseCompleted> Cases => cases;

        public int Passed => summary.Passed;
        public int Failed => summary.Failed;
        public int Skipped => summary.Skipped;
        public TimeSpan Duration => summary.Duration;
    }
}