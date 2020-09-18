namespace Fixie.TestAdapter
{
    using System;

    class RunnerException : Exception
    {
        public RunnerException(Exception exception)
            : base(exception.ToString())
        {
        }
    }
}