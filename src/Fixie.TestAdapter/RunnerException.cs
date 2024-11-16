namespace Fixie.TestAdapter;

class RunnerException : Exception
{
    public RunnerException(Exception exception)
        : base(exception.ToString())
    {
    }
}