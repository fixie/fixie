using System;
using System.Text;

namespace Fixie.Execution
{
    [Serializable]
    public class ExecutionSummary
    {
        public ExecutionSummary()
        {
            Duration = TimeSpan.Zero;
            Passed = 0;
            Failed = 0;
            Skipped = 0;
        }

        public TimeSpan Duration { get; set; }
        public int Passed { get; set; }
        public int Failed { get; set; }
        public int Skipped { get; set; }
        public int Total => Passed + Failed + Skipped;

        public void Include(CaseCompleted message)
        {
            Duration += message.Duration;

            if (message.Status == CaseStatus.Passed)
                Passed += 1;
            else if (message.Status == CaseStatus.Failed)
                Failed += 1;
            else if (message.Status == CaseStatus.Skipped)
                Skipped += 1;
        }

        public override string ToString()
        {
            var summary = new StringBuilder();

            summary.Append($"{Passed} passed");
            summary.Append($", {Failed} failed");

            if (Skipped > 0)
                summary.Append($", {Skipped} skipped");

            summary.Append($", took {Duration.TotalSeconds:N2} seconds");

            return summary.ToString();
        }
    }
}