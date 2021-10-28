namespace Fixie.Internal
{
    using System;
    using System.Diagnostics;

    class ExecutingTest
    {
        readonly Stopwatch stopwatch;

        public ExecutingTest(Test test)
        {
            Test = test;
            stopwatch = new Stopwatch();
            stopwatch.Start();
        }

        public Test Test { get; }
        public TimeSpan Elapsed => stopwatch.Elapsed;
    }
}
