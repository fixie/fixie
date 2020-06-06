namespace Fixie.TestAdapter
{
    using System;
    using System.Text;

    public class RunnerException : Exception
    {
        public RunnerException(Exception exception)
            : base(new StringBuilder()
                .AppendLine()
                .AppendLine()
                .AppendLine(exception.Message)
                .AppendLine()
                .AppendLine(exception.GetType().FullName)
                .AppendLine(exception.StackTrace)
                .ToString())
        {
        }
    }
}