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
            log.RecordResult(
                FullyQualifiedName(message),
                message.Name,
                message.Status.ToString(),
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
                message.Status.ToString(),
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
                message.Status.ToString(),
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