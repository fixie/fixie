using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Fixie.Reports;

namespace Fixie.Internal;

static class PipeMessage
{
    public static string Serialize<TMessage>(TMessage message)
    {
        return JsonSerializer.Serialize(message);
    }

    public static TMessage Deserialize<TMessage>(string json)
    {
        var message = JsonSerializer.Deserialize<TMessage>(json);

        if (message == null)
            throw new System.Exception($"Message of type {typeof(TMessage).FullName} was unexpectedly null.");

        return message;
    }

    public class DiscoverTests { }

    public class ExecuteTests
    {
        public required string[] Filter { get; init; }
    }

    public class TestDiscovered
    {
        public required string Test { get; init; }
    }

    public class TestStarted
    {
        public required string Test { get; init; }
    }

    public abstract class TestCompleted
    {
        public required string Test { get; init; }
        public required string TestCase { get; init; }
        public required double DurationInMilliseconds { get; init; }
    }

    public class TestSkipped : TestCompleted
    {
        public required string Reason { get; init; }
    }

    public class TestPassed : TestCompleted
    {
    }

    public class TestFailed : TestCompleted
    {
        public required Exception Reason { get; init; }
    }

    public class Exception
    {
        public Exception()
        {
        }

        [SetsRequiredMembers]
        public Exception(System.Exception exception)
        {
            Type = exception.GetType().FullName!;
            Message = exception.Message;
            StackTrace = exception.StackTraceSummary();
        }

        public required string Type { get; init; }
        public required string Message { get; init; }
        public required string StackTrace { get; init; }
    }

    public class EndOfPipe { }
}