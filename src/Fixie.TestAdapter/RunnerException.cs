namespace Fixie.TestAdapter
{
    using System;
    using System.Text;
    using Reports;

    class RunnerException : Exception
    {
        public RunnerException(Exception exception)
            : base(exception.ToString())
        {
        }

        public RunnerException(PipeMessage.Exception exception)
            : base(new StringBuilder()
                .AppendLine()
                .AppendLine()
                .AppendLine(exception.Message)
                .AppendLine()
                .AppendLine(exception.Type)
                .AppendLine(exception.StackTrace)
                .ToString())
        {
        }
    }
}