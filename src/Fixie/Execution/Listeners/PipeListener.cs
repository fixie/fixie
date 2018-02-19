namespace Fixie.Execution.Listeners
{
    using System;
    using System.IO.Pipes;
    using Execution;

    public class PipeListener :
        Handler<MethodDiscovered>,
        Handler<CaseSkipped>,
        Handler<CasePassed>,
        Handler<CaseFailed>
    {
        readonly NamedPipeClientStream pipe;

        public PipeListener(NamedPipeClientStream pipe)
        {
            this.pipe = pipe;
        }

        public void Handle(MethodDiscovered message)
        {
            var methodGroup = new MethodGroup(message.Method);

            Write(new PipeMessage.Test
            {
                FullyQualifiedName = methodGroup.FullName,
                DisplayName = methodGroup.FullName
            });
        }

        public void Handle(CaseSkipped message)
        {
            Write(message, x =>
            {
                x.Outcome = "Skipped";
                x.ErrorMessage = message.Reason;
            });
        }

        public void Handle(CasePassed message)
        {
            Write(message, x =>
            {
                x.Outcome = "Passed";
            });
        }

        public void Handle(CaseFailed message)
        {
            Write(message, x =>
            {
                x.Outcome = "Failed";
                x.ErrorMessage = message.Message;
                x.ErrorStackTrace = message.TypedStackTrace();
            });
        }

        void Write(CaseCompleted message, Action<PipeMessage.TestResult> customize)
        {
            var testResult = new PipeMessage.TestResult
            {
                FullyQualifiedName = new MethodGroup(message.Method).FullName,
                DisplayName = message.Name,
                Duration = message.Duration,
                Output = message.Output
            };

            customize(testResult);

            Write(testResult);
        }

        void Write<T>(T message) => pipe.Send(message);
    }
}