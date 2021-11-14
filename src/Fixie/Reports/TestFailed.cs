namespace Fixie.Reports;

using System;

public class TestFailed : TestCompleted
{
    internal TestFailed(string test, string testCase, TimeSpan duration, string output, Exception reason)
        : base(test, testCase, duration, output)
    {
        Reason = reason;
    }

    public Exception Reason { get; }
}