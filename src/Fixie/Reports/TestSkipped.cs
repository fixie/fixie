namespace Fixie.Reports
{
    using System;

    public class TestSkipped : TestCompleted
    {
        internal TestSkipped(string test, string name, TimeSpan duration, string output, string? reason)
            : base(test, name, duration, output)
        {
            Reason = reason;
        }

        public string? Reason { get; }
    }
}