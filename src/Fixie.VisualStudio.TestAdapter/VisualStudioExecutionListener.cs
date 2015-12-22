using System;
using Fixie.Execution;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

namespace Fixie.VisualStudio.TestAdapter
{
    public class VisualStudioExecutionListener : LongLivedMarshalByRefObject, IHandler<CaseCompleted>
    {
        readonly ITestExecutionRecorder log;
        readonly string assemblyPath;

        public VisualStudioExecutionListener(ITestExecutionRecorder log, string assemblyPath)
        {
            this.log = log;
            this.assemblyPath = assemblyPath;
        }

        public void Handle(CaseCompleted message)
        {
            var testResult = new TestResult(TestCase(message.MethodGroup))
            {
                DisplayName = message.Name,
                Outcome = Map(message.Status),
                Duration = message.Duration,
                ComputerName = Environment.MachineName
            };

            if (message.Status == CaseStatus.Failed)
            {
                testResult.ErrorMessage = message.AssertionFailed ? "" : message.ExceptionType;
                testResult.ErrorStackTrace = message.Message + Environment.NewLine + Environment.NewLine + message.StackTrace;
            }
            else if (message.Status == CaseStatus.Skipped)
            {
                testResult.ErrorMessage = message.Message;
            }

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