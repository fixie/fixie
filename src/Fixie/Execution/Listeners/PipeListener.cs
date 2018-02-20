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
            pipe.Send(new PipeMessage.SkipResult
            {
                FullName = new MethodGroup(message.Method).FullName,
                DisplayName = message.Name,
                Duration = message.Duration,
                Output = message.Output,
                ErrorMessage = message.Reason
            });
        }

        public void Handle(CasePassed message)
        {
            pipe.Send(new PipeMessage.PassResult
            {
                FullName = new MethodGroup(message.Method).FullName,
                DisplayName = message.Name,
                Duration = message.Duration,
                Output = message.Output
            });
        }

        public void Handle(CaseFailed message)
        {
            pipe.Send(new PipeMessage.FailResult
            {
                FullName = new MethodGroup(message.Method).FullName,
                DisplayName = message.Name,
                Duration = message.Duration,
                Output = message.Output,
                ErrorMessage = message.Exception.Message,
                ErrorStackTrace = message.Exception.TypeName() +
                                  NewLine +
                                  message.Exception.CompoundStackTrace()
            });
        }
    }
}