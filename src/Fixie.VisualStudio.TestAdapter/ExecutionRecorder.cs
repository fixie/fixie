namespace Fixie.VisualStudio.TestAdapter
{
    using System;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
    using Execution.Listeners;
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

        public void RecordResult(PipeMessage.SkipResult result)
        {
            RecordResult(result, x =>
            {
                x.Outcome = TestOutcome.Skipped;
                x.ErrorMessage = result.Reason;
            });
        }

        public void RecordResult(PipeMessage.PassResult result)
        {
            RecordResult(result, x =>
            {
                x.Outcome = TestOutcome.Passed;
            });
        }

        public void RecordResult(PipeMessage.FailResult result)
        {
            RecordResult(result, x =>
            {
                x.Outcome = TestOutcome.Failed;
                x.ErrorMessage = result.Exception.Message;
                x.ErrorStackTrace = result.Exception.TypeName +
                                    NewLine +
                                    result.Exception.StackTrace;
            });
        }

        void RecordResult(PipeMessage.TestResult result, Action<TestResult> customize = null)
        {
            var testCase = new TestCase(result.FullName, VsTestExecutor.Uri, assemblyPath);

            var testResult = new TestResult(testCase)
            {
                DisplayName = result.DisplayName,
                Duration = result.Duration,
                ComputerName = MachineName
            };

            customize?.Invoke(testResult);

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