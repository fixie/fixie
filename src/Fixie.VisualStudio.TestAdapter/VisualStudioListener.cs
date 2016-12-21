namespace Fixie.VisualStudio.TestAdapter
{
    using System;
    using Execution;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

    public class VisualStudioListener : LongLivedMarshalByRefObject, Listener
    {
        readonly ITestExecutionRecorder log;
        readonly string assemblyPath;

        public VisualStudioListener(ITestExecutionRecorder log, string assemblyPath)
        {
            this.log = log;
            this.assemblyPath = assemblyPath;
        }

        public void AssemblyStarted(AssemblyStarted message) { }

        public void CaseSkipped(CaseSkipped message)
        {
            log.RecordResult(new TestResult(TestCase(message.MethodGroup))
            {
                DisplayName = message.Name,
                Outcome = Map(CaseStatus.Skipped),
                ComputerName = Environment.MachineName,
                ErrorMessage = message.SkipReason
            });
        }

        public void CasePassed(CasePassed message)
        {
            var testResult = new TestResult(TestCase(message.MethodGroup))
            {
                DisplayName = message.Name,
                Outcome = Map(CaseStatus.Passed),
                Duration = message.Duration,
                ComputerName = Environment.MachineName
            };

            AttachCapturedConsoleOutput(message.Output, testResult);

            log.RecordResult(testResult);
        }

        public void CaseFailed(CaseFailed message)
        {
            var testResult = new TestResult(TestCase(message.MethodGroup))
            {
                DisplayName = message.Name,
                Outcome = Map(CaseStatus.Failed),
                Duration = message.Duration,
                ComputerName = Environment.MachineName,
                ErrorMessage = message.Exceptions.PrimaryException.DisplayName,
                ErrorStackTrace = message.Exceptions.CompoundStackTrace
            };

            AttachCapturedConsoleOutput(message.Output, testResult);

            log.RecordResult(testResult);
        }

        public void AssemblyCompleted(AssemblyCompleted message)
        {
        }

        TestCase TestCase(MethodGroup methodGroup)
        {
            return new TestCase(methodGroup.FullName, VsTestExecutor.Uri, assemblyPath);
        }

        static TestOutcome Map(CaseStatus caseStatus)
        {
            switch (caseStatus)
            {
                case CaseStatus.Passed:
                    return TestOutcome.Passed;
                case CaseStatus.Failed:
                    return TestOutcome.Failed;
                case CaseStatus.Skipped:
                    return TestOutcome.Skipped;
                default:
                    return TestOutcome.None;
            }
        }

        static void AttachCapturedConsoleOutput(string output, TestResult testResult)
        {
            if (!String.IsNullOrEmpty(output))
                testResult.Messages.Add(new TestResultMessage(TestResultMessage.StandardOutCategory, output));
        }
    }
}