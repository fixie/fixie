namespace Fixie.VisualStudio.TestAdapter
{
    using System;
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
                fullyQualifiedName: new MethodGroup(message.Class, message.Method).FullName,
                displayName: message.Name,
                outcome: message.Status.ToString(),
                duration: message.Duration,
                output: message.Output,
                errorMessage: message.Reason,
                errorStackTrace: null
            );
        }

        public void Handle(CasePassed message)
        {
            log.RecordResult(
                fullyQualifiedName: new MethodGroup(message.Class, message.Method).FullName,
                displayName: message.Name,
                outcome: message.Status.ToString(),
                duration: message.Duration,
                output: message.Output,
                errorMessage: null,
                errorStackTrace: null
            );
        }

        public void Handle(CaseFailed message)
        {
            var exception = message.Exception;

            log.RecordResult(
                fullyQualifiedName: new MethodGroup(message.Class, message.Method).FullName,
                displayName: message.Name,
                outcome: message.Status.ToString(),
                duration: message.Duration,
                output: message.Output,
                errorMessage: exception.Message,
                errorStackTrace: exception.TypedStackTrace()
            );
        }
    }
}