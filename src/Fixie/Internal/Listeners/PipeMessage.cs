namespace Fixie.Internal.Listeners
{
    using System;

    // Because we only deserialize message instances that had been constructed
    // and serialized in a null-safe fashion, deserialization is trusted. Whenever
    // a default constructor is needed only to satisfy the deserializer, it is
    // declared both private and empty, with corresponding default! per property.

    public static class PipeMessage
    {
        public class Test
        {
            public Test(Fixie.Test test)
            {
                Class = test.Class;
                Method = test.Method;
                Name = test.Name;
            }

            Test() { /* Trust Deserialization */ }

            public string Class { get; set; } = default!;
            public string Method { get; set; } = default!;
            public string Name { get; set; } = default!;
        }

        public class DiscoverTests { }

        public class ExecuteTests
        {
            public ExecuteTests()
                : this(Array.Empty<Test>()) { }

            public ExecuteTests(Test[] filter)
            {
                Filter = filter;
            }

            public Test[] Filter { get; set; }
        }

        public class TestDiscovered
        {
            public TestDiscovered(MethodDiscovered message)
                => Test = new Test(new Fixie.Test(message.Method));

            TestDiscovered() { /* Trust Deserialization */ }

            public Test Test { get; set; } = default!;
        }

        public class CaseStarted
        {
            public CaseStarted(Internal.CaseStarted message)
            {
                Test = new Test(new Fixie.Test(message.Method));
                Name = message.Name;
            }

            CaseStarted() { /* Trust Deserialization */ }

            public Test Test { get; set; } = default!;
            public string Name { get; set; } = default!;
        }

        public abstract class CaseCompleted
        {
            protected CaseCompleted(Internal.CaseCompleted message)
            {
                var test = new Fixie.Test(message.Method);

                Test = new Test(test);
                Name = message.Name;
                Duration = message.Duration;
                Output = message.Output;
            }

            protected CaseCompleted() { /* Trust Deserialization */ }

            public Test Test { get; set; } = default!;
            public string Name { get; set; } = default!;
            public TimeSpan Duration { get; set; }
            public string Output { get; set; } = default!;
        }

        public class CaseSkipped : CaseCompleted
        {
            public CaseSkipped(Internal.CaseSkipped message)
                : base(message)
            {
                Reason = message.Reason;
            }

            CaseSkipped() { /* Trust Deserialization */ }

            public string? Reason { get; set; }
        }

        public class CasePassed : CaseCompleted
        {
            public CasePassed(Internal.CasePassed message)
                :base(message) { }

            CasePassed() { /* Trust Deserialization */ }
        }

        public class CaseFailed : CaseCompleted
        {
            public CaseFailed(Internal.CaseFailed message)
                : base(message)
            {
                Exception = new Exception(message.Exception);
            }

            public CaseFailed(CaseStarted caseStarted, Exception exception)
            {
                Test = caseStarted.Test;
                Name = caseStarted.Name;
                Output = "";
                Duration = TimeSpan.Zero;
                Exception = exception;
            }

            CaseFailed() { /* Trust Deserialization */ }

            public Exception Exception { get; set; } = default!;
        }

        public class Exception
        {
            public Exception(System.Exception exception)
            {
                Type = exception.GetType().FullName!;
                Message = exception.Message;
                StackTrace = exception.LiterateStackTrace();
            }

            public Exception(string type, string message, string stackTrace)
            {
                Type = type;
                Message = message;
                StackTrace = stackTrace;
            }

            Exception() { /* Trust Deserialization */ }

            public string Type { get; set; } = default!;
            public string Message { get; set; } = default!;
            public string StackTrace { get; set; } = default!;
        }

        public class Completed { }
    }
}