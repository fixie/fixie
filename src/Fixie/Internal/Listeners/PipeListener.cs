namespace Fixie.Internal.Listeners
{
    using System.IO.Pipes;
    using Internal;

    class PipeListener :
        Handler<MethodDiscovered>,
        Handler<CaseStarted>,
        Handler<CaseSkipped>,
        Handler<CasePassed>,
        Handler<CaseFailed>
    {
        readonly NamedPipeClientStream pipe;

        public PipeListener(NamedPipeClientStream pipe)
        {
            this.pipe = pipe;
        }

        public void Handle(MethodDiscovered message) => pipe.Send(new PipeMessage.TestDiscovered(message));
        public void Handle(CaseStarted message) => pipe.Send(new PipeMessage.CaseStarted(message));
        public void Handle(CaseSkipped message) => pipe.Send(new PipeMessage.CaseSkipped(message));
        public void Handle(CasePassed message) => pipe.Send(new PipeMessage.CasePassed(message));
        public void Handle(CaseFailed message) => pipe.Send(new PipeMessage.CaseFailed(message));
    }
}