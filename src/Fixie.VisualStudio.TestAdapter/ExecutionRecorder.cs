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

        public void RecordResult(PipeMessage.TestResult result)
        {
            var testCase = new TestCase(result.FullName, VsTestExecutor.Uri, assemblyPath);

            var testResult = new TestResult(testCase)
            {
                DisplayName = result.DisplayName,
                Outcome = Parse(result.Outcome),
                Duration = result.Duration,
                ComputerName = Environment.MachineName,

                ErrorMessage = result.ErrorMessage,
                ErrorStackTrace = result.ErrorStackTrace
            };

            AttachCapturedConsoleOutput(result.Output, testResult);

            log.RecordResult(testResult);
        }

        static TestOutcome Parse(string outcome)
            => Enum.TryParse(outcome, out TestOutcome parsed) ? parsed : TestOutcome.None;

        static void AttachCapturedConsoleOutput(string output, TestResult testResult)
        {
            if (!String.IsNullOrEmpty(output))
                testResult.Messages.Add(new TestResultMessage(TestResultMessage.StandardOutCategory, output));
        }
    }
}