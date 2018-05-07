namespace Fixie.VisualStudio.TestAdapter
{
    using System;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
    using Internal.Listeners;
    using static System.Environment;

    public class ExecutionRecorder
    {
        readonly ITestExecutionRecorder log;
        readonly string assemblyPath;

        public ExecutionRecorder(ITestExecutionRecorder log, string assemblyPath)
        {
            this.log = log;
            this.assemblyPath = assemblyPath;
        }

        public void Record(PipeMessage.CaseSkipped result)
        {
            Record(result, x =>
            {
                x.Outcome = TestOutcome.Skipped;
                x.ErrorMessage = result.Reason;
            });
        }

        public void Record(PipeMessage.CasePassed result)
        {
            Record(result, x =>
            {
                x.Outcome = TestOutcome.Passed;
            });
        }

        public void Record(PipeMessage.CaseFailed result)
        {
            Record(result, x =>
            {
                x.Outcome = TestOutcome.Failed;
                x.ErrorMessage = result.Exception.Message;
                x.ErrorStackTrace = result.Exception.Type +
                                    NewLine +
                                    result.Exception.StackTrace;
            });
        }

        void Record(PipeMessage.CaseCompleted result, Action<TestResult> customize = null)
        {
            var testCase = ToVsTestCase(result.Test);

            var testResult = new TestResult(testCase)
            {
                DisplayName = result.Name,
                Duration = result.Duration,
                ComputerName = MachineName
            };

            customize?.Invoke(testResult);

            AttachCapturedConsoleOutput(result.Output, testResult);

            log.RecordResult(testResult);
        }

        TestCase ToVsTestCase(PipeMessage.Test test)
        {
            return new TestCase(test.Name, VsTestExecutor.Uri, assemblyPath);
        }

        static void AttachCapturedConsoleOutput(string output, TestResult testResult)
        {
            if (!String.IsNullOrEmpty(output))
                testResult.Messages.Add(new TestResultMessage(TestResultMessage.StandardOutCategory, output));
        }
    }
}