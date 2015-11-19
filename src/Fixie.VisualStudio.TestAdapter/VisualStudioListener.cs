using System;
using Fixie.Execution;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

namespace Fixie.VisualStudio.TestAdapter
{
    // Subscribers to the Fixie message bus are constructed within the child
    // AppDomain, but we ultimately need to pass test case results into Visual
    // Studio's own ITestExecutionRecorder, which is constructed by Visual
    // Studio in the original AppDomain and which is not declared to be a
    // MarshalByRefObject.
    //
    // We solve this problem with one layer of indirection. We can wrap the
    // ITestExecutionRecorder in a class that *is* a MarshalByRefObject and
    // therefore that wrapper can effectively be "passed" to the child AppDomain.
    //
    // VisualStudioListener truly lives within the child AppDomain,
    // VisualStudioListenerProxy truly lives within the original AppDomain
    // with Visual Studio's ITestExecutionRecorder, and the CaseResult messages
    // themselves are successfully passed from the child AppDomain to the original
    // because they are serializable.
    //
    // In other words, a CaseResult published in the child AppDomain can be
    // successfully ushered to the original AppDomain so that it can be reported
    // to Visual Studio's own test result recording interface.

    public class VisualStudioListener : IHandler<CaseResult>
    {
        readonly VisualStudioListenerProxy proxy;

        public VisualStudioListener(VisualStudioListenerProxy proxy)
        {
            this.proxy = proxy;
        }

        public void Handle(CaseResult message)
            => proxy.Handle(message);
    }

    public class VisualStudioListenerProxy : LongLivedMarshalByRefObject, IHandler<CaseResult>
    {
        readonly ITestExecutionRecorder log;
        readonly string assemblyPath;

        public VisualStudioListenerProxy(ITestExecutionRecorder log, string assemblyPath)
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