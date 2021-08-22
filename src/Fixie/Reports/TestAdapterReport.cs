namespace Fixie.Reports
{
    using System;
    using System.Threading.Tasks;

    public class TestAdapterReport :
        IHandler<TestDiscovered>,
        IHandler<TestStarted>,
        IHandler<TestSkipped>,
        IHandler<TestPassed>,
        IHandler<TestFailed>
    {
        readonly TestAdapterPipe pipe;

        public TestAdapterReport(TestAdapterPipe pipe)
        {
            this.pipe = pipe;
        }

        public Task Handle(TestDiscovered message)
        {
            Write(new PipeMessage.TestDiscovered
            {
                Test = message.Test
            });

            return Task.CompletedTask;
        }

        public Task Handle(TestStarted message)
        {
            Write(new PipeMessage.TestStarted
            {
                Test = message.Test
            });

            return Task.CompletedTask;
        }

        public Task Handle(TestSkipped message)
        {
            Write<PipeMessage.TestSkipped>(message, x =>
            {
                x.Reason = message.Reason;
            });

            return Task.CompletedTask;
        }

        public Task Handle(TestPassed message)
        {
            Write<PipeMessage.TestPassed>(message);

            return Task.CompletedTask;
        }

        public Task Handle(TestFailed message)
        {
            Write<PipeMessage.TestFailed>(message, x =>
            {
                x.Reason = new PipeMessage.Exception(message.Reason);
            });

            return Task.CompletedTask;
        }

        void Write<TTestResult>(TestCompleted message, Action<TTestResult>? customize = null)
            where TTestResult : PipeMessage.TestCompleted, new()
        {
            var result = new TTestResult
            {
                Test = message.Test,
                TestCase = message.TestCase,
                DurationInMilliseconds = message.Duration.TotalMilliseconds,
                Output = message.Output
            };

            customize?.Invoke(result);

            Write(result);
        }

        void Write<T>(T message) => pipe.Send(message);
    }
}
