using System;
using System.Reflection;
using Fixie.Execution;
using Fixie.Results;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

namespace Fixie.VisualStudio.TestAdapter
{
    public class VisualStudioListener : MarshalByRefObject, Listener
    {
        readonly ITestExecutionRecorder log;
        readonly string source;

        public VisualStudioListener(ITestExecutionRecorder log, string source)
        {
            this.log = log;
            this.source = source;
        }

        public void AssemblyStarted(string assemblyFileName) { }

        public void CaseSkipped(SkipResult result)
        {
            var testCase = new TestCase(result.Name, Executor.Uri, source);
            log.RecordResult(new TestResult(testCase)
            {
                DisplayName = result.Name,
                Outcome = Map(CaseStatus.Passed),
                ComputerName = Environment.MachineName
            });
        }

        public void CasePassed(PassResult result)
        {
            var testCase = new TestCase(result.Name, Executor.Uri, source);
            log.RecordResult(new TestResult(testCase)
            {
                DisplayName = result.Name,
                Outcome = Map(CaseStatus.Passed),
                Duration = result.Duration,
                ComputerName = Environment.MachineName
            });
        }

        public void CaseFailed(FailResult result)
        {
            var testCase = new TestCase(result.Name, Executor.Uri, source);
            log.RecordResult(new TestResult(testCase)
            {
                DisplayName = result.Name,
                Outcome = Map(CaseStatus.Failed),
                Duration = result.Duration,
                ComputerName = Environment.MachineName,
                ErrorMessage = result.Exceptions.PrimaryException.DisplayName,
                ErrorStackTrace = result.Exceptions.CompoundStackTrace
            });
        }

        public void AssemblyCompleted(string assemblyFileName, AssemblyResult result) { }

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
    }
}