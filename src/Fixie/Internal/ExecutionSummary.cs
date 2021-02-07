namespace Fixie.Internal
{
    class ExecutionSummary
    {
        public ExecutionSummary()
        {
            Passed = 0;
            Failed = 0;
            Skipped = 0;
        }

        public int Passed { get; private set; }
        public int Failed { get; private set; }
        public int Skipped { get; private set; }
        public int Total => Passed + Failed + Skipped;

        public void Add(CaseSkipped message) => Skipped += 1;
        public void Add(CasePassed message) => Passed += 1;
        public void Add(CaseFailed message) => Failed += 1;
    }
}