namespace Fixie.TestAdapter
{
    using System;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
    using Reports;
    using static System.Environment;

    class ExecutionReport :
        Handler<TestStarted>,
        Handler<TestSkipped>,
        Handler<TestPassed>,
        Handler<TestFailed>
    {
        readonly ITestExecutionRecorder log;
        readonly string assemblyPath;

        public ExecutionReport(ITestExecutionRecorder log, string assemblyPath)
        {
            this.log = log;
            this.assemblyPath = assemblyPath;
        }

        public void Handle(TestStarted message)
        {
            var testCase = ToVsTestCase(message.Test);

            log.RecordStart(testCase);
        }

        public void Handle(TestSkipped message)
        {
            Record(message, x =>
            {
                x.Outcome = TestOutcome.Skipped;
                x.ErrorMessage = message.Reason;
            });
        }

        public void Handle(TestPassed message)
        {
            Record(message, x =>
            {
                x.Outcome = TestOutcome.Passed;
            });
        }

        public void Handle(TestFailed message)
        {
            Record(message, x =>
            {
                x.Outcome = TestOutcome.Failed;
                x.ErrorMessage = message.Exception.Message;
                x.ErrorStackTrace = message.Exception.GetType().FullName +
                                    NewLine +
                                    message.Exception.LiterateStackTrace();
            });
        }

        void Record(TestCompleted result, Action<TestResult> customize)
        {
            var testCase = ToVsTestCase(result.Test);

            var testResult = new TestResult(testCase)
            {
                DisplayName = result.Name,
                Duration = result.Duration,
                ComputerName = MachineName
            };

            customize(testResult);

            AttachCapturedConsoleOutput(result.Output, testResult);

            log.RecordResult(testResult);
        }

        TestCase ToVsTestCase(string fullyQualifiedName)
        {
            return new TestCase(fullyQualifiedName, VsTestExecutor.Uri, assemblyPath);
        }

        static void AttachCapturedConsoleOutput(string output, TestResult testResult)
        {
            if (!string.IsNullOrEmpty(output))
                testResult.Messages.Add(new TestResultMessage(TestResultMessage.StandardOutCategory, output));
        }
    }
}