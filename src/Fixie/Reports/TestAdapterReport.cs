using Fixie.Internal;

namespace Fixie.Reports;

class TestAdapterReport(TestEnvironment environment, TestAdapterPipe pipe) :
    IHandler<TestDiscovered>,
    IHandler<TestStarted>,
    IHandler<TestSkipped>,
    IHandler<TestPassed>,
    IHandler<TestFailed>
{
    readonly SourceLocationProvider sourceLocationProvider = new(environment.Assembly.Location);

    public Task Handle(TestDiscovered message)
    {
        SourceLocation? sourceLocation = null;

        try
        {
            sourceLocationProvider.TryGetSourceLocation(message.Test, out sourceLocation);
        }
        catch (Exception exception)
        {
            using (Foreground.Yellow)
                environment.Console.WriteLine(
                    $"{GetType().FullName} threw an exception while " +
                    $"attempting to handle a message of type {typeof(TestDiscovered).FullName}:");
            environment.Console.WriteLine();
            environment.Console.WriteLine(exception.ToString());
            environment.Console.WriteLine();
        }

        pipe.Send(new PipeMessage.TestDiscovered
        {
            Test = message.Test,
            SourceLocation = sourceLocation
        });

        return Task.CompletedTask;
    }

    public Task Handle(TestStarted message)
    {
        pipe.Send(new PipeMessage.TestStarted
        {
            Test = message.Test
        });

        return Task.CompletedTask;
    }

    public Task Handle(TestSkipped message)
    {
        pipe.Send(new PipeMessage.TestSkipped
        {
            Test = message.Test,
            TestCase = message.TestCase,
            DurationInMilliseconds = message.Duration.TotalMilliseconds,
            Reason = message.Reason
        });

        return Task.CompletedTask;
    }

    public Task Handle(TestPassed message)
    {
        pipe.Send(new PipeMessage.TestPassed
        {
            Test = message.Test,
            TestCase = message.TestCase,
            DurationInMilliseconds = message.Duration.TotalMilliseconds
        });

        return Task.CompletedTask;
    }

    public Task Handle(TestFailed message)
    {
        pipe.Send(new PipeMessage.TestFailed
        {
            Test = message.Test,
            TestCase = message.TestCase,
            DurationInMilliseconds = message.Duration.TotalMilliseconds,
            Reason = new PipeMessage.Exception(message.Reason)
        });

        return Task.CompletedTask;
    }
}