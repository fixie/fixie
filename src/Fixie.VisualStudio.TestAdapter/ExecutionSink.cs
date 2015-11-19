using System;
using Fixie.Execution;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

namespace Fixie.VisualStudio.TestAdapter
{
    public class ExecutionSink : LongLivedMarshalByRefObject, IHandler<CaseResult>
    {
        readonly ITestExecutionRecorder log;
        readonly string assemblyPath;

        public ExecutionSink(ITestExecutionRecorder log, string assemblyPath)
        {
            this.log = log;
            this.assemblyPath = assemblyPath;
        }

        public void Handle(CaseResult caseResult)
        {
            var testResult = new TestResult(TestCase(caseResult.MethodGroup))
            {
                DisplayName = caseResult.Name,
                Outcome = Map(caseResult.Status),
                Duration = caseResult.Duration,
                ComputerName = Environment.MachineName
            };

            if (caseResult.Status == CaseStatus.Failed)
            {
                testResult.ErrorMessage = caseResult.Exceptions.PrimaryException.DisplayName;
                testResult.ErrorStackTrace = caseResult.Exceptions.CompoundStackTrace;
            }
            else if (caseResult.Status == CaseStatus.Passed)
            {
                testResult.ErrorMessage = null;
                testResult.ErrorStackTrace = null;
            }
            else if (caseResult.Status == CaseStatus.Skipped)
            {
                testResult.ErrorMessage = caseResult.SkipReason;
                testResult.ErrorStackTrace = null;
            }

            AttachCapturedConsoleOutput(caseResult.Output, testResult);

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