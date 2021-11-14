namespace Fixie.Reports;

using System;

public class TestSkipped : TestCompleted
{
    internal TestSkipped(string test, string testCase, TimeSpan duration, string output, string reason)
        : base(test, testCase, duration, output)
    {
        Reason = reason;
    }

    public string Reason { get; }
}