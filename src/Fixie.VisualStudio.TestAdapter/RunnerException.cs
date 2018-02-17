namespace Fixie.VisualStudio.TestAdapter
{
    using System;
    using Execution.Listeners;
    using static System.Environment;

    public class RunnerException : Exception
    {
        public RunnerException(PipeMessage.Exception exception)
            : base($"{NewLine}{NewLine}{exception.Details}{NewLine}")
        {
        }
    }
}