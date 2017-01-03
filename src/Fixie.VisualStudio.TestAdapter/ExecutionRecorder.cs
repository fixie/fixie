namespace Fixie.VisualStudio.TestAdapter
{
    using System;
    using Execution;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

    public class ExecutionRecorder : LongLivedMarshalByRefObject
    {
        readonly ITestExecutionRecorder log;
        readonly string assemblyPath;

        public ExecutionRecorder(ITestExecutionRecorder log, string assemblyPath)
        {
            this.log = log;
            this.assemblyPath = assemblyPath;
        }

        public void RecordResult(Result result)
        {
            var testCase = new TestCase(result.FullyQualifiedName, VsTestExecutor.Uri, assemblyPath);

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
        {
            TestOutcome parsed;

            if (Enum.TryParse(outcome, out parsed))
                return parsed;

            return TestOutcome.None;
        }

        static void AttachCapturedConsoleOutput(string output, TestResult testResult)
        {
            if (!String.IsNullOrEmpty(output))
                testResult.Messages.Add(new TestResultMessage(TestResultMessage.StandardOutCategory, output));
        }
    }
}