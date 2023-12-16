using Fixie.Reports;

namespace Fixie.Internal;

/*
 * Nullability hints here are informed by usages. Although these types are serialized and deserialized
 * to and from JSON, and in theory could be null upon deserialization, we can trace all real
 * deserialization from corresponding serialization where no nulls are in play.
 */

static class PipeMessage
{
    public class DiscoverTests { }

    public class ExecuteTests
    {
        public string[] Filter { get; init; } = default!;
    }

    public class TestDiscovered
    {
        public string Test { get; init; } = default!;
    }

    public class TestStarted
    {
        public string Test { get; init; } = default!;
    }

    public abstract class TestCompleted
    {
        public string Test { get; init; } = default!;
        public string TestCase { get; init; } = default!;
        public double DurationInMilliseconds { get; init; }
        public string Output { get; init; } = default!;
    }

    public class TestSkipped : TestCompleted
    {
        public string Reason { get; init; } = default!;
    }

    public class TestPassed : TestCompleted
    {
    }

    public class TestFailed : TestCompleted
    {
        public Exception Reason { get; init; } = default!;
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

        public string Type { get; init; } = default!;
        public string Message { get; init; } = default!;
        public string StackTrace { get; init; } = default!;
    }

    public class EndOfPipe { }
}