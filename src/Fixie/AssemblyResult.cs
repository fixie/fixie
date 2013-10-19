using System;

namespace Fixie
{
    [Serializable]
    public class AssemblyResult
    {
        readonly int passed;
        readonly int failed;

        public AssemblyResult(int passed, int failed)
        {
            this.passed = passed;
            this.failed = failed;
        }

        public int Passed { get { return passed; } }

        public int Failed { get { return failed; } }

        public int Total
        {
            get { return Passed + Failed; }
        }
    }
}