namespace Fixie.VisualStudio.TestAdapter
{
    using Execution;

    public class VisualStudioExecutionListener :
        Handler<CaseSkipped>,
        Handler<CasePassed>,
        Handler<CaseFailed>
    {
        readonly IExecutionRecorder log;

        public VisualStudioExecutionListener(IExecutionRecorder log)
        {
            this.log = log;
        }

        public void Handle(CaseSkipped message)
        {
            log.RecordResult(
                FullyQualifiedName(message),
                message.Name,
                "Skipped",
                message.Duration,
                message.Output,
                message.Reason,
                errorStackTrace: null
            );
        }

        public void Handle(CasePassed message)
        {
            log.RecordResult(
                FullyQualifiedName(message),
                message.Name,
                "Passed",
                message.Duration,
                message.Output,
                errorMessage: null,
                errorStackTrace: null
            );
        }

        public void Handle(CaseFailed message)
        {
            var exception = message.Exception;

            log.RecordResult(
                FullyQualifiedName(message),
                message.Name,
                "Failed",
                message.Duration,
                message.Output,
                exception.Message,
                exception.TypedStackTrace()
            );
        }

        static string FullyQualifiedName(CaseCompleted message)
            => new MethodGroup(message.Class, message.Method).FullName;
    }
}