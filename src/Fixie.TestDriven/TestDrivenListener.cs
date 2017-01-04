using TestDriven.Framework;

namespace Fixie.TestDriven
{
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
        }

        public void Handle(CaseSkipped message)
        {
            tdnet.TestFinished(new TestResult
            {
                Name = message.Name,
                State = TestState.Ignored,
                Message = message.SkipReason
            });
        }

        public void Handle(CasePassed message)
        {
            tdnet.TestFinished(new TestResult
            {
                Name = message.Name,
                State = TestState.Passed
            });
        }

        public void Handle(CaseFailed message)
        {
            tdnet.TestFinished(new TestResult
            {
                Name = message.Name,
                State = TestState.Failed,
                Message = message.Exceptions.PrimaryException.DisplayName,
                StackTrace = message.Exceptions.CompoundStackTrace,
            });
        }
    }
}