namespace Fixie.Reports;

/// <summary>
/// Fired when an individual test has passed.
/// </summary>
public class TestPassed : TestCompleted
{
    internal TestPassed(string test, string testCase, TimeSpan duration, string output)
        : base(test, testCase, duration, output)
    {
    }
}