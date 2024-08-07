﻿namespace Fixie.Reports;

/// <summary>
/// Fired when an individual test has been skipped.
/// </summary>
public class TestSkipped : TestCompleted
{
    internal TestSkipped(string test, string testCase, TimeSpan duration, string reason)
        : base(test, testCase, duration)
    {
        Reason = reason;
    }

    /// <summary>
    /// An explanation for why the test was skipped.
    /// </summary>
    public string Reason { get; }
}