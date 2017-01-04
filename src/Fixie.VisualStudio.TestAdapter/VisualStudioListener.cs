namespace Fixie.VisualStudio.TestAdapter
{
    using Execution;

    public class VisualStudioListener :
        Handler<CaseSkipped>,
        Handler<CasePassed>,
        Handler<CaseFailed>
    {
        readonly ExecutionRecorder log;

        public VisualStudioListener(ExecutionRecorder log)
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
                ErrorMessage = message.SkipReason,
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
            log.RecordResult(new Result
            {
                FullyQualifiedName = new MethodGroup(message.Method).FullName,
                DisplayName = message.Name,
                Outcome = message.Status.ToString(),
                Duration = message.Duration,
                Output = message.Output,
                ErrorMessage = message.Exceptions.PrimaryException.DisplayName,
                ErrorStackTrace = message.Exceptions.CompoundStackTrace
            });
        }
    }
}