namespace Fixie.Reports;

public abstract class TestCompleted : IMessage
{
    internal TestCompleted(string test, string testCase, TimeSpan duration)
    {
        Test = test;
        TestCase = testCase;
        Duration = duration;
    }

    /// <summary>
    /// The name of the test.
    /// </summary>
    public string Test { get; }
        
    /// <summary>
    /// The name of the specific invocation of the test.
    /// For most tests, this is the same as the `Test` property. For
    /// parameterized tests, the test case includes additional information
    /// about the input parameters in effect for this invocation of the
    /// `Test`.
    /// </summary>
    public string TestCase { get; }
        
    /// <summary>
    /// The duration of the test execution.
    /// </summary>
    public TimeSpan Duration { get; }
}