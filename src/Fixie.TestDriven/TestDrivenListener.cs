using TestDriven.Framework;

namespace Fixie.TestDriven
{
    using Execution;

    public class TestDrivenListener : Listener
    {
        readonly ITestListener tdnet;

        public TestDrivenListener(ITestListener tdnet)
        {
            this.tdnet = tdnet;
        }

        public void AssemblyStarted(AssemblyStarted message)
        {
        }

        public void CaseSkipped(CaseSkipped message)
        {
            tdnet.TestFinished(new TestResult
            {
                Name = message.Name,
                State = TestState.Ignored,
                Message = message.SkipReason
            });
        }

        public void CasePassed(CasePassed message)
        {
            tdnet.TestFinished(new TestResult
            {
                Name = message.Name,
                State = TestState.Passed
            });
        }

        public void CaseFailed(CaseFailed message)
        {
            tdnet.TestFinished(new TestResult
            {
                Name = message.Name,
                State = TestState.Failed,
                Message = message.Exceptions.PrimaryException.DisplayName,
                StackTrace = message.Exceptions.CompoundStackTrace,
            });
        }

        public void AssemblyCompleted(AssemblyCompleted message)
        {
        }
    }
}