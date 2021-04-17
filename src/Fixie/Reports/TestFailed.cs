namespace Fixie.Reports
{
    using System;

    public class TestFailed : TestCompleted
    {
        internal TestFailed(string test, string name, TimeSpan duration, string output, Exception reason)
            : base(test, name, duration, output)
        {
            Reason = reason;
        }

        public Exception Reason { get; }
    }
}