namespace Fixie.Reports
{
    using System;

    public static class PipeMessage
    {
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
            public TimeSpan Duration { get; set; }
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
                StackTrace = exception.LiterateStackTrace();
            }

            public string Type { get; set; } = default!;
            public string Message { get; set; } = default!;
            public string StackTrace { get; set; } = default!;
        }

        public class Completed { }
    }
}