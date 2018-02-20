namespace Fixie.VisualStudio.TestAdapter
{
    using System;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
    using Execution.Listeners;

    public class ExecutionRecorder
    {
        readonly ITestExecutionRecorder log;
        readonly string assemblyPath;

        public ExecutionRecorder(ITestExecutionRecorder log, string assemblyPath)
        {
            this.log = log;
            this.assemblyPath = assemblyPath;
        }

        public void RecordResult(PipeMessage.SkipResult result)
        {
            var testCase = new TestCase(result.FullName, VsTestExecutor.Uri, assemblyPath);

            var testResult = new TestResult(testCase)
            {
                DisplayName = result.DisplayName,
                Outcome = TestOutcome.Skipped,
                Duration = result.Duration,
                ComputerName = Environment.MachineName,

                ErrorMessage = result.ErrorMessage,
                ErrorStackTrace = null
            };

            AttachCapturedConsoleOutput(result.Output, testResult);

            log.RecordResult(testResult);
        }

        public void RecordResult(PipeMessage.PassResult result)
        {
            var testCase = new TestCase(result.FullName, VsTestExecutor.Uri, assemblyPath);

            var testResult = new TestResult(testCase)
            {
                DisplayName = result.DisplayName,
                Outcome = TestOutcome.Passed,
                Duration = result.Duration,
                ComputerName = Environment.MachineName,

                ErrorMessage = null,
                ErrorStackTrace = null
            };

            AttachCapturedConsoleOutput(result.Output, testResult);

            log.RecordResult(testResult);
        }

        public void RecordResult(PipeMessage.FailResult result)
        {
            var testCase = new TestCase(result.FullName, VsTestExecutor.Uri, assemblyPath);

            var testResult = new TestResult(testCase)
            {
                DisplayName = result.DisplayName,
                Outcome = TestOutcome.Failed,
                Duration = result.Duration,
                ComputerName = Environment.MachineName,

                ErrorMessage = result.ErrorMessage,
                ErrorStackTrace = result.ErrorStackTrace
            };

            AttachCapturedConsoleOutput(result.Output, testResult);

            log.RecordResult(testResult);
        }

        static void AttachCapturedConsoleOutput(string output, TestResult testResult)
        {
            if (!String.IsNullOrEmpty(output))
                testResult.Messages.Add(new TestResultMessage(TestResultMessage.StandardOutCategory, output));
        }
    }
}