namespace Fixie.Runner
{
    using System;
    using Contracts;
    using Execution;

    public class DesignTimeExecutionListener :
        Handler<CaseSkipped>,
        Handler<CasePassed>,
        Handler<CaseFailed>
    {
        readonly IDesignTimeSink sink;

        public DesignTimeExecutionListener(IDesignTimeSink sink)
        {
            this.sink = sink;
        }

        public void Handle(CaseSkipped message)
        {
            Log(message, x =>
            {
                x.ErrorMessage = message.Reason;
            });
        }

        public void Handle(CasePassed message)
        {
            Log(message);
        }

        public void Handle(CaseFailed message)
        {
            var exception = message.Exception;

            Log(message, x =>
            {
                x.ErrorMessage = exception.Message;
                x.ErrorStackTrace = exception.TypedStackTrace();
            });
        }

        void Log(CaseCompleted message, Action<TestResult> customize = null)
        {
            var methodGroup = message.MethodGroup.FullName;

            var test = new Test
            {
                FullyQualifiedName = methodGroup,
                DisplayName = methodGroup
            };

            var testResult = new TestResult(test)
            {
                Outcome = (TestOutcome)Enum.Parse(typeof(TestOutcome), message.Status.ToString()),
                DisplayName = message.Name,
                ComputerName = Environment.MachineName,
                Duration = message.Duration,
            };

            AttachCapturedConsoleOutput(message.Output, testResult);

            customize?.Invoke(testResult);

            sink.SendTestStarted(test);//TODO: Send this message at the actual start of each test case.
            sink.SendTestResult(testResult);
        }

        static void AttachCapturedConsoleOutput(string output, TestResult testResult)
        {
            if (!String.IsNullOrEmpty(output))
                testResult.Messages.Add(output);
        }
    }
}