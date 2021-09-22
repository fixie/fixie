namespace Fixie.Internal
{
    using System;
    using System.Threading;
    using Reports;

    class ExecutionSummary
    {
        int passed;
        int failed;
        int skipped;

        public ExecutionSummary()
        {
            passed = 0;
            failed = 0;
            skipped = 0;
        }

        public int Passed => Interlocked.CompareExchange(ref passed, 0, 0);
        public int Failed => Interlocked.CompareExchange(ref failed, 0, 0);
        public int Skipped => Interlocked.CompareExchange(ref skipped, 0, 0);
        public int Total => Passed + Failed + Skipped;

        public void Add(TestPassed message) => Interlocked.Increment(ref passed);
        public void Add(TestFailed message) => Interlocked.Increment(ref failed);
        public void Add(TestSkipped message) => Interlocked.Increment(ref skipped);
    }
}