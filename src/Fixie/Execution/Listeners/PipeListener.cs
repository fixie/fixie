namespace Fixie.Execution.Listeners
{
    using System.IO.Pipes;
    using Execution;
    using static System.Environment;

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

            pipe.Send(new PipeMessage.Test
            {
                FullName = methodGroup.FullName,
                DisplayName = methodGroup.FullName
            });
        }

        public void Handle(CaseSkipped message)
        {
            pipe.Send(new PipeMessage.TestResult
            {
                FullName = new MethodGroup(message.Method).FullName,
                DisplayName = message.Name,
                Duration = message.Duration,
                Output = message.Output,
                Outcome = "Skipped",
                ErrorMessage = message.Reason
            });
        }

        public void Handle(CasePassed message)
        {
            pipe.Send(new PipeMessage.TestResult
            {
                FullName = new MethodGroup(message.Method).FullName,
                DisplayName = message.Name,
                Duration = message.Duration,
                Output = message.Output,
                Outcome = "Passed"
            });
        }

        public void Handle(CaseFailed message)
        {
            pipe.Send(new PipeMessage.TestResult
            {
                FullName = new MethodGroup(message.Method).FullName,
                DisplayName = message.Name,
                Duration = message.Duration,
                Output = message.Output,
                Outcome = "Failed",
                ErrorMessage = message.Exception.Message,
                ErrorStackTrace = message.Exception.TypeName() +
                                  NewLine +
                                  message.Exception.CompoundStackTrace()
            });
        }
    }
}