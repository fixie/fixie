namespace Fixie.Execution
{
    using System;
    using System.Collections.Generic;

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

        public void Add(CaseSkipped message)
        {
            Skipped += 1;
            Duration += message.Duration;
        }

        public void Add(CasePassed message)
        {
            Passed += 1;
            Duration += message.Duration;
        }

        public void Add(CaseFailed message)
        {
            Failed += 1;
            Duration += message.Duration;
        }

        public void Add(ExecutionSummary partial)
        {
            Passed += partial.Passed;
            Failed += partial.Failed;
            Skipped += partial.Skipped;
            Duration += partial.Duration;
        }

        public override string ToString()
        {
            if (Total == 0)
                return "No tests found.";

            var parts = new List<string>();

            if (Passed > 0)
                parts.Add($"{Passed} passed");

            if (Failed > 0)
                parts.Add($"{Failed} failed");

            if (Skipped > 0)
                parts.Add($"{Skipped} skipped");

            parts.Add($"took {Duration.TotalSeconds:N2} seconds");

            return String.Join(", ", parts);
        }
    }
}