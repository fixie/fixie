using Fixie.Internal;

namespace Fixie.Reports;

class TestAdapterReport :
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
        var result = new PipeMessage.TestSkipped
        {
            Test = message.Test,
            TestCase = message.TestCase,
            DurationInMilliseconds = message.Duration.TotalMilliseconds,
            Output = message.Output,
            Reason = message.Reason
        };

        Write(result);

        return Task.CompletedTask;
    }

    public Task Handle(TestPassed message)
    {
        var result = new PipeMessage.TestPassed
        {
            Test = message.Test,
            TestCase = message.TestCase,
            DurationInMilliseconds = message.Duration.TotalMilliseconds,
            Output = message.Output
        };

        Write(result);

        return Task.CompletedTask;
    }

    public Task Handle(TestFailed message)
    {
        var result = new PipeMessage.TestFailed
        {
            Test = message.Test,
            TestCase = message.TestCase,
            DurationInMilliseconds = message.Duration.TotalMilliseconds,
            Output = message.Output,
            Reason = new PipeMessage.Exception(message.Reason)
        };

        Write(result);

        return Task.CompletedTask;
    }

    void Write<T>(T message) => pipe.Send(message);
}