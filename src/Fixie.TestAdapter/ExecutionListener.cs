namespace Fixie.TestAdapter
{
    using System;
    using Internal;
    using Internal.Listeners;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
    using static System.Environment;

    class ExecutionListener :
        Handler<CaseStarted>,
        Handler<CaseSkipped>,
        Handler<CasePassed>,
        Handler<CaseFailed>
    {
        readonly ITestExecutionRecorder log;
        readonly string assemblyPath;

        public ExecutionListener(ITestExecutionRecorder log, string assemblyPath)
        {
            this.log = log;
            this.assemblyPath = assemblyPath;
        }

        public void Handle(CaseStarted message)
        {
            var test = new Test(message.Method);
            var testCase = ToVsTestCase(test);

            log.RecordStart(testCase);
        }

        public void Handle(CaseSkipped message)
        {
            Record(message, x =>
            {
                x.Outcome = TestOutcome.Skipped;
                x.ErrorMessage = message.Reason;
            });
        }

        public void Handle(CasePassed message)
        {
            Record(message, x =>
            {
                x.Outcome = TestOutcome.Passed;
            });
        }

        public void Handle(CaseFailed message)
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

        void Record(CaseCompleted result, Action<TestResult> customize)
        {
            var test = new Test(result.Method);
            var testCase = ToVsTestCase(test);

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

        TestCase ToVsTestCase(Test test)
        {
            return new TestCase(test.Name, VsTestExecutor.Uri, assemblyPath);
        }

        static void AttachCapturedConsoleOutput(string output, TestResult testResult)
        {
            if (!string.IsNullOrEmpty(output))
                testResult.Messages.Add(new TestResultMessage(TestResultMessage.StandardOutCategory, output));
        }
    }
}