namespace Fixie.Reports;

/// <summary>
/// Fired when an individual test has failed.
/// </summary>
public class TestFailed : TestCompleted
{
    internal TestFailed(string test, string testCase, TimeSpan duration, Exception reason)
        : base(test, testCase, duration)
    {
        Reason = reason;
    }

    /// <summary>
    /// The uncaught exception indicating test failure, such as from a failed assertion.
    /// </summary>
    public Exception Reason { get; }
}