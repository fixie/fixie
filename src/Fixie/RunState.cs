namespace Fixie
{
    public class RunState
    {
        int passed;
        int failed;

        public void CasePassed()
        {
            passed++;
        }

        public void CaseFailed()
        {
            failed++;
        }

        public Result ToResult()
        {
            return new Result(passed, failed);
        }
    }
}