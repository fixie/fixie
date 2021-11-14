namespace Fixie.Reports;

using System;

public class TestPassed : TestCompleted
{
    internal TestPassed(string test, string testCase, TimeSpan duration, string output)
        : base(test, testCase, duration, output)
    {
    }
}