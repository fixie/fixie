using System.Text.Json;
using System.Text.Json.Serialization;
using Fixie.Reports;

namespace Fixie.Internal;

/*
 * Nullability hints here are informed by usages. Although these types are serialized and deserialized
 * to and from JSON, and in theory could be null upon deserialization, we can trace all real
 * deserialization from corresponding serialization where no nulls are in play.
 */

[JsonSerializable(typeof(PipeMessage.DiscoverTests))]
[JsonSerializable(typeof(PipeMessage.ExecuteTests))]
[JsonSerializable(typeof(PipeMessage.TestDiscovered))]
[JsonSerializable(typeof(PipeMessage.TestStarted))]
[JsonSerializable(typeof(PipeMessage.TestSkipped))]
[JsonSerializable(typeof(PipeMessage.TestPassed))]
[JsonSerializable(typeof(PipeMessage.TestFailed))]
[JsonSerializable(typeof(PipeMessage.Exception))]
[JsonSerializable(typeof(PipeMessage.EndOfPipe))]
partial class PipeMessageContext : JsonSerializerContext
{
}

static class PipeMessage
{
    public static string Serialize<TMessage>(TMessage message)
    {
        return JsonSerializer.Serialize(message, typeof(TMessage), PipeMessageContext.Default);
    }

    public static TMessage? Deserialize<TMessage>(string json) where TMessage: class
    {
        return JsonSerializer.Deserialize(json, typeof(TMessage), PipeMessageContext.Default) as TMessage;
    }

    public class DiscoverTests { }

    public class ExecuteTests
    {
        public string[] Filter { get; set; } = default!;
    }

    public class TestDiscovered
    {
        public string Test { get; set; } = default!;
    }

    public class TestStarted
    {
        public string Test { get; set; } = default!;
    }

    public abstract class TestCompleted
    {
        public string Test { get; set; } = default!;
        public string TestCase { get; set; } = default!;
        public double DurationInMilliseconds { get; set; }
        public string Output { get; set; } = default!;
    }

    public class TestSkipped : TestCompleted
    {
        public string Reason { get; set; } = default!;
    }

    public class TestPassed : TestCompleted
    {
    }

    public class TestFailed : TestCompleted
    {
        public Exception Reason { get; set; } = default!;
    }

    public class Exception
    {
        public Exception()
        {
        }

        public Exception(System.Exception exception)
        {
            Type = exception.GetType().FullName!;
            Message = exception.Message;
            StackTrace = exception.StackTraceSummary();
        }

        public string Type { get; set; } = default!;
        public string Message { get; set; } = default!;
        public string StackTrace { get; set; } = default!;
    }

    public class EndOfPipe { }
}