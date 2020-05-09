namespace Fixie.Internal.Listeners
{
    using System;

    public static class PipeMessage
    {
        public class Test
        {
            public string Class { get; set; }
            public string Method { get; set; }
            public string Name { get; set; }
        }

        public class DiscoverTests { }

        public class ExecuteTests
        {
            public Test[] Filter { get; set; }
        }

        public class TestDiscovered
        {
            public Test Test { get; set; }
        }

        public class CaseStarted
        {
            public Test Test { get; set; }
            public string Name { get; set; }
        }

        public abstract class CaseCompleted
        {
            public Test Test { get; set; }
            public string Name { get; set; }
            public TimeSpan Duration { get; set; }
            public string Output { get; set; }
        }

        public class CaseSkipped : CaseCompleted
        {
            public string? Reason { get; set; }
        }

        public class CasePassed : CaseCompleted
        {
        }

        public class CaseFailed : CaseCompleted
        {
            public Exception Exception { get; set; }
        }

        public class Exception
        {
            public Exception()
            {
            }

            public Exception(System.Exception exception)
            {
                Type = exception.GetType().FullName;
                Message = exception.Message;
                StackTrace = exception.LiterateStackTrace();
            }

            public string Type { get; set; }
            public string Message { get; set; }
            public string StackTrace { get; set; }
        }

        public class Completed { }
    }
}