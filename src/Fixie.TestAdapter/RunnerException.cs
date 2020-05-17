namespace Fixie.TestAdapter
{
    using System;
    using System.Text;
    using Internal.Listeners;

    public class RunnerException : Exception
    {
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