namespace Fixie.Internal.Listeners
{
    using System;
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

        public void Handle(MethodDiscovered message)
        {
            var test = new Test(message.Method);

            Write(new PipeMessage.TestDiscovered
            {
                Test = new PipeMessage.Test
                {
                    Class = test.Class,
                    Method = test.Method,
                    Name = test.Name
                }
            });
        }

        public void Handle(CaseStarted message)
        {
            var test = new Test(message.Method);

            Write(new PipeMessage.CaseStarted
            {
                Test = new PipeMessage.Test
                {
                    Class = test.Class,
                    Method = test.Method,
                    Name = test.Name
                },

                Name = message.Name,
            });
        }

        public void Handle(CaseSkipped message)
        {
            Write<PipeMessage.CaseSkipped>(message, x =>
            {
                x.Reason = message.Reason;
            });
        }

        public void Handle(CasePassed message)
        {
            Write<PipeMessage.CasePassed>(message);
        }

        public void Handle(CaseFailed message)
        {
            Write<PipeMessage.CaseFailed>(message, x =>
            {
                x.Exception = new PipeMessage.Exception(message.Exception);
            });
        }

        void Write<TTestResult>(CaseCompleted message, Action<TTestResult> customize = null)
            where TTestResult : PipeMessage.CaseCompleted, new()
        {
            var test = new Test(message.Method);

            var result = new TTestResult
            {
                Test = new PipeMessage.Test
                {
                    Class = test.Class,
                    Method = test.Method,
                    Name = test.Name
                },

                Name = message.Name,
                Duration = message.Duration,
                Output = message.Output
            };

            customize?.Invoke(result);

            Write(result);
        }

        void Write<T>(T message) => pipe.Send(message);
    }
}