namespace Fixie
{
    public class Result
    {
        public Result(int passed, int failed)
        {
            Passed = passed;
            Failed = failed;
        }

        public int Total { get { return Passed + Failed; } }
        public int Passed { get; private set; }
        public int Failed { get; private set; }
    }
}