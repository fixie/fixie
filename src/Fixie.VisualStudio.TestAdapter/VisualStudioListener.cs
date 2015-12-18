using System;
using Fixie.Execution;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

namespace Fixie.VisualStudio.TestAdapter
{
    // Subscribers to the Fixie message bus are usually constructed within the child
    // AppDomain, partly to minimize the need for busy chatter across the AppDomain
    // boundary and partly so that they can handle message types with AppDomain-
    // unfriendly properties.
    //
    // Unfortunately, the Visual Studio runner ultimately needs to report test results
    // to Visual Studio through an ITestExecutionRecorder that is constructed by
    // Visual Studio back in the original AppDomain. Visual Studio's recorder object
    // is not a MarshalByRefObject, so it cannot simply be passed to the child
    // AppDomain where all the test result messages are published and handled.
    //
    // We solve this problem with one layer of indirection. We can wrap the
    // ITestExecutionRecorder in a class that *is* a MarshalByRefObject and
    // therefore that wrapper can effectively be "passed" to the child AppDomain as a
    // subscriber of test result messages. The wrapper truly lives in the original
    // AppDomain, while the child AppDomain works against a proxy of it.
    //
    // The test run takes place in the child AppDomain, and test result messages
    // are published and received by handlers within the child AppDomain. Our
    // proxy really receives the messages, which are then serialized and passed
    // to the real VisualStudioListener in the original AppDomain, which can then
    // pass them to Visual Studio's ITestExecutionRecorder.
    //
    // This only works under the following constraints:
    //     1. VisualStudioListener must be a LongLivedMarshalByRefObject.
    //     2. VisualStudioListener must be constructed in the original
    //        AppDomain and "passed" into the ExecutionEnvironment.
    //     3. VisualStudioListener can only be an IHandler<T> for serializable T.
    //     4. CaseResult is guaranteed to remain serializable precisely to support
    //        cross-AppDomain communication)

    public class VisualStudioListener : LongLivedMarshalByRefObject, IHandler<CaseResult>
    {
        readonly ITestExecutionRecorder log;
        readonly string assemblyPath;

        public VisualStudioListener(ITestExecutionRecorder log, string assemblyPath)
        {
            this.log = log;
            this.assemblyPath = assemblyPath;
        }

        public void Handle(CaseResult message)
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
            else if (message.Status == CaseStatus.Passed)
            {
                testResult.ErrorMessage = null;
                testResult.ErrorStackTrace = null;
            }
            else if (message.Status == CaseStatus.Skipped)
            {
                testResult.ErrorMessage = message.Message;
                testResult.ErrorStackTrace = null;
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