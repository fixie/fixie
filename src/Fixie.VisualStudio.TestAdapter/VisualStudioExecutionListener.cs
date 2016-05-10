using System;
using Fixie.Execution;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

namespace Fixie.VisualStudio.TestAdapter
{
    public class VisualStudioExecutionListener :
        Handler<CaseSkipped>,
        Handler<CasePassed>,
        Handler<CaseFailed>
    {
        readonly ITestExecutionRecorder log;
        readonly string assemblyPath;

        public VisualStudioExecutionListener(ITestExecutionRecorder log, string assemblyPath)
        {
            this.log = log;
            this.assemblyPath = assemblyPath;
        }

        public void Handle(CaseSkipped message)
        {
            log.RecordResult(new TestResult(TestCase(message.MethodGroup))
            {
                DisplayName = message.Name,
                Outcome = Map(CaseStatus.Skipped),
                ComputerName = Environment.MachineName,
                ErrorMessage = message.SkipReason
            });
        }

        public void Handle(CasePassed message)
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

        public void Handle(CaseFailed message)
        {
            var testResult = new TestResult(TestCase(message.MethodGroup))
            {
                DisplayName = message.Name,
                Outcome = Map(CaseStatus.Failed),
                Duration = message.Duration,
                ComputerName = Environment.MachineName,
                ErrorMessage = message.Exceptions.PrimaryException.Message,
                ErrorStackTrace = message.Exceptions.PrimaryException.Type + Environment.NewLine + message.Exceptions.CompoundStackTrace
            };

            AttachCapturedConsoleOutput(message.Output, testResult);

            log.RecordResult(testResult);
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