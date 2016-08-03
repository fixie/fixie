namespace Fixie.VisualStudio.TestAdapter
{
    using System;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
    using Execution;

    public class VisualStudioExecutionListener : LongLivedMarshalByRefObject,
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
            Log(message, x =>
            {
                x.ErrorMessage = message.Reason;
            });
        }

        public void Handle(CasePassed message)
        {
            Log(message);
        }

        public void Handle(CaseFailed message)
        {
            var exception = message.Exception;

            Log(message, x =>
            {
                x.ErrorMessage = exception.Message;
                x.ErrorStackTrace = TypedStackTrace(exception);
            });
        }

        static string TypedStackTrace(CompoundException exception)
        {
            if (exception.FailedAssertion)
                return exception.StackTrace;

            return exception.Type + Environment.NewLine + exception.StackTrace;
        }

        void Log(CaseCompleted message, Action<TestResult> customize = null)
        {
            var testResult = new TestResult(TestCase(message.MethodGroup))
            {
                DisplayName = message.Name,
                Outcome = Map(message.Status),
                Duration = message.Duration,
                ComputerName = Environment.MachineName
            };

            AttachCapturedConsoleOutput(message.Output, testResult);

            customize?.Invoke(testResult);

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