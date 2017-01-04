namespace Fixie.Execution
{
    using System;
    using System.Text;

    public class ExecutionSummary
    {
        public ExecutionSummary()
        {
            Passed = 0;
            Failed = 0;
            Skipped = 0;
            Duration = TimeSpan.Zero;
        }

        public int Passed { get; private set; }
        public int Failed { get; private set; }
        public int Skipped { get; private set; }
        public int Total => Passed + Failed + Skipped;
        public TimeSpan Duration { get; private set; }

        public void Add(CaseCompleted message)
        {
            if (message.Status == CaseStatus.Passed)
                Passed += 1;
            else if (message.Status == CaseStatus.Failed)
                Failed += 1;
            else if (message.Status == CaseStatus.Skipped)
                Skipped += 1;

            Duration += message.Duration;
        }
    }
}