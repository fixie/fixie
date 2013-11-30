using System;

namespace Fixie
{
    [Serializable]
    public class AssemblyResult
    {
        readonly int passed;
        readonly int failed;
        readonly int skipped;

        public AssemblyResult(int passed, int skipped, int failed)
        {
            this.passed = passed;
            this.failed = failed;
            this.skipped = skipped;
        }

        public int Passed { get { return passed; } }

        public int Skipped { get { return skipped; } }

        public int Failed { get { return failed; } }

        public int Total
        {
            get { return Passed + Skipped + Failed; }
        }
    }
}