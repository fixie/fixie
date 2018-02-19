using TestDriven.Framework;

namespace Fixie.TestDriven
{
    using System;
    using Execution;
    using static System.Environment;

    public class TestDrivenListener :
        Handler<CaseSkipped>,
        Handler<CasePassed>,
        Handler<CaseFailed>
    {
        readonly ITestListener tdnet;

        public TestDrivenListener(ITestListener tdnet)
        {
            this.tdnet = tdnet;
        }

        public void Handle(CaseSkipped message)
        {
            Log(message, x =>
            {
                x.State = TestState.Ignored;
                x.Message = message.Reason;
            });
        }

        public void Handle(CasePassed message)
        {
            Log(message, x =>
            {
                x.State = TestState.Passed;
            });
        }

        public void Handle(CaseFailed message)
        {
            Log(message, x =>
            {
                x.State = TestState.Failed;
                x.Message = message.FailedAssertion ? "" : message.Type;
                x.StackTrace = message.Message + NewLine + NewLine + message.StackTrace;
            });
        }

        void Log(CaseCompleted message, Action<TestResult> customize)
        {
            var testResult = new TestResult { Name = message.Name };

            customize(testResult);

            tdnet.TestFinished(testResult);
        }
    }
}