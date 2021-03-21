namespace Fixie.Reports
{
    using System;
    using Internal;

    public class TestFailed : TestCompleted
    {
        internal TestFailed(Case @case, TimeSpan duration, string output, Exception reason)
            : base(@case, duration, output)
        {
            Reason = reason;
        }

        public Exception Reason { get; }
    }
}