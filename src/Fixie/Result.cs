namespace Fixie
{
    public class Result
    {
        public static readonly Result Pass = new Result(passed: 1, failed: 0);
        public static readonly Result Fail = new Result(passed: 0, failed: 1);

        public Result() : this(0, 0) { }

        public Result(int passed, int failed)
        {
            Passed = passed;
            Failed = failed;
        }

        public int Total
        {
            get { return Passed + Failed; }
        }

        public int Passed { get; private set; }
        public int Failed { get; private set; }

        public static Result Combine(Result a, Result b)
        {
            return new Result(a.Passed + b.Passed, a.Failed + b.Failed);
        }
    }
}