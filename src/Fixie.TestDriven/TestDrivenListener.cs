using TestDriven.Framework;

namespace Fixie.TestDriven
{
    using System;
    using Execution;

    public class TestDrivenListener :
        Handler<CaseSkipped>,
        Handler<CasePassed>,
        Handler<CaseFailed>
    {
        readonly ITestListener tdnet;

        public TestDrivenListener(ITestListener tdnet)
        {
            this.tdnet = tdnet;
            Summary = new ExecutionSummary();
        }

        public ExecutionSummary Summary { get; }

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
                x.Message = message.Exception.FailedAssertion ? "" : message.Exception.Type;
                x.StackTrace = message.Exception.Message + Environment.NewLine + Environment.NewLine + message.Exception.StackTrace;
            });
        }

        void Log(CaseCompleted message, Action<TestResult> customize)
        {
            Summary.Add(message);

            var testResult = new TestResult { Name = message.Name };

            customize?.Invoke(testResult);

            tdnet.TestFinished(testResult);
        }
    }
}