namespace Fixie.Execution.Listeners
{
    using System;
    using System.IO.Pipes;
    using System.Reflection;
    using Execution;

    public class TestExplorerListener :
        Handler<MethodDiscovered>,
        Handler<CaseSkipped>,
        Handler<CasePassed>,
        Handler<CaseFailed>,
        Handler<AssemblyCompleted>
    {
        readonly NamedPipeClientStream pipe;

        public TestExplorerListener()
        {
            var pipeName = Environment.GetEnvironmentVariable("FIXIE_NAMED_PIPE");

            pipe = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut)
            {
                ReadMode = PipeTransmissionMode.Message
            };

            pipe.Connect();
        }

        public void Handle(MethodDiscovered message)
        {
            var methodGroup = new MethodGroup(message.Class, message.Method);

            Write(new Test
            {
                FullyQualifiedName = methodGroup.FullName,
                DisplayName = methodGroup.FullName
            });
        }

        public void Handle(CaseSkipped message)
        {
            Write(message, x =>
            {
                x.Outcome = "Skipped";
                x.ErrorMessage = message.Reason;
            });
        }

        public void Handle(CasePassed message)
        {
            Write(message, x =>
            {
                x.Outcome = "Passed";
            });
        }

        public void Handle(CaseFailed message)
        {
            var exception = message.Exception;

            Write(message, x =>
            {
                x.Outcome = "Failed";
                x.ErrorMessage = exception.Message;
                x.ErrorStackTrace = exception.TypedStackTrace();
            });
        }

        public void Handle(AssemblyCompleted message)
        {
            Write(new Completed());
            pipe.Dispose();
        }

        void Write(CaseCompleted message, Action<TestResult> customize)
        {
            var testResult = new TestResult
            {
                FullyQualifiedName = FullyQualifiedName(message.Class, message.Method),
                DisplayName = message.Name,
                Duration = message.Duration,
                Output = message.Output
            };

            customize(testResult);

            Write(testResult);
        }

        static string FullyQualifiedName(Type @class, MethodInfo method)
            => new MethodGroup(@class, method).FullName;

        void Write<T>(T message)
        {
            pipe.SendMessage(typeof(T).FullName);
            pipe.Send(message);
        }

        public class Test
        {
            public string FullyQualifiedName { get; set; }
            public string DisplayName { get; set; }
        }

        public class TestResult
        {
            public string FullyQualifiedName { get; set; }
            public string DisplayName { get; set; }
            public string Outcome { get; set; }
            public TimeSpan Duration { get; set; }
            public string Output { get; set; }
            public string ErrorMessage { get; set; }
            public string ErrorStackTrace { get; set; }
        }

        public class Completed
        {
        }
    }
}