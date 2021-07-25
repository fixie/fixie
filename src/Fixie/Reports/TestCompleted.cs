namespace Fixie.Reports
{
    using System;

    public abstract class TestCompleted : Message
    {
        internal TestCompleted(string test, string testCase, TimeSpan duration, string output)
        {
            Test = test;
            TestCase = testCase;
            Duration = duration;
            Output = output;
        }

        public string Test { get; }
        public string TestCase { get; }
        public TimeSpan Duration { get; }
        public string Output { get; }
    }
}