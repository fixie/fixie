using System;

namespace Fixie
{
    [Serializable]
    public class AssemblyResult
    {
        readonly int passed;
        readonly int failed;
        readonly int skipped;

        public AssemblyResult(int passed, int failed, int skipped)
        {
            this.passed = passed;
            this.failed = failed;
            this.skipped = skipped;
        }

        public int Passed { get { return passed; } }

        public int Failed { get { return failed; } }

        public int Skipped { get { return skipped; } }

        public int Total
        {
            get { return Passed + Failed + Skipped; }
        }
    }
}