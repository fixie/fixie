using System;
using Fixie.Execution;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

namespace Fixie.VisualStudio.TestAdapter
{
    public class VisualStudioListener : Listener
    {
        readonly ITestExecutionRecorder log;
        readonly string assemblyPath;

        public VisualStudioListener(ITestExecutionRecorder log, string assemblyPath)
        {
            this.log = log;
            this.assemblyPath = assemblyPath;
        }

        public void AssemblyStarted(AssemblyInfo assembly) { }

        public void CaseSkipped(SkipResult result)
        {
            log.RecordResult(new TestResult(TestCase(result.MethodGroup))
            {
                DisplayName = result.Name,
                Outcome = Map(CaseStatus.Skipped),
                ComputerName = Environment.MachineName,
                ErrorMessage = result.SkipReason
            });
        }

        public void CasePassed(PassResult result)
        {
            var testResult = new TestResult(TestCase(result.MethodGroup))
            {
                DisplayName = result.Name,
                Outcome = Map(CaseStatus.Passed),
                Duration = result.Duration,
                ComputerName = Environment.MachineName
            };

            AttachCapturedConsoleOutput(result.Output, testResult);

            log.RecordResult(testResult);
        }

        public void CaseFailed(FailResult result)
        {
            var testResult = new TestResult(TestCase(result.MethodGroup))
            {
                DisplayName = result.Name,
                Outcome = Map(CaseStatus.Failed),
                Duration = result.Duration,
                ComputerName = Environment.MachineName,
                ErrorMessage = result.Exceptions.PrimaryException.DisplayName,
                ErrorStackTrace = result.Exceptions.CompoundStackTrace
            };

            AttachCapturedConsoleOutput(result.Output, testResult);

            log.RecordResult(testResult);
        }

        public void AssemblyCompleted(AssemblyInfo assembly, AssemblyResult result)
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