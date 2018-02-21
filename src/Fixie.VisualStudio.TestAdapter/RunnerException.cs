namespace Fixie.VisualStudio.TestAdapter
{
    using System;
    using System.Text;
    using Execution.Listeners;

    public class RunnerException : Exception
    {
        public RunnerException(PipeMessage.Exception exception)
            : base(new StringBuilder()
                .AppendLine()
                .AppendLine()
                .AppendLine(exception.Message)
                .AppendLine()
                .AppendLine(exception.TypeName)
                .AppendLine(exception.StackTrace)
                .ToString())
        {
        }
    }
}