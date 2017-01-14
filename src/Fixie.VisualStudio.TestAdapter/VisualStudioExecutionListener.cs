namespace Fixie.VisualStudio.TestAdapter
{
    using Execution;

    public class VisualStudioExecutionListener :
        Handler<CaseSkipped>,
        Handler<CasePassed>,
        Handler<CaseFailed>
    {
        readonly ExecutionRecorder log;

        public VisualStudioExecutionListener(ExecutionRecorder log)
        {
            this.log = log;
        }

        public void Handle(CaseSkipped message)
        {
            log.RecordResult(new Result
            {
                FullyQualifiedName = new MethodGroup(message.Method).FullName,
                DisplayName = message.Name,
                Outcome = message.Status.ToString(),
                Duration = message.Duration,
                Output = message.Output,
                ErrorMessage = message.Reason,
                ErrorStackTrace = null
            });
        }

        public void Handle(CasePassed message)
        {
            log.RecordResult(new Result
            {
                FullyQualifiedName = new MethodGroup(message.Method).FullName,
                DisplayName = message.Name,
                Outcome = message.Status.ToString(),
                Duration = message.Duration,
                Output = message.Output,
                ErrorMessage = null,
                ErrorStackTrace = null
            });
        }

        public void Handle(CaseFailed message)
        {
            var exception = message.Exception;

            log.RecordResult(new Result
            {
                FullyQualifiedName = new MethodGroup(message.Method).FullName,
                DisplayName = message.Name,
                Outcome = message.Status.ToString(),
                Duration = message.Duration,
                Output = message.Output,
                ErrorMessage = exception.Message,
                ErrorStackTrace = exception.TypedStackTrace()
            });
        }
    }
}