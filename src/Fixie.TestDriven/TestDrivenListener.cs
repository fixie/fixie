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

        public void AssemblyStarted(AssemblyInfo message)
        {
        }

        public void CaseSkipped(SkipResult message)
        {
            tdnet.TestFinished(new TestResult
            {
                Name = message.Name,
                State = TestState.Ignored,
                Message = message.SkipReason
            });
        }

        public void CasePassed(PassResult message)
        {
            tdnet.TestFinished(new TestResult
            {
                Name = message.Name,
                State = TestState.Passed
            });
        }

        public void CaseFailed(FailResult message)
        {
            tdnet.TestFinished(new TestResult
            {
                Name = message.Name,
                State = TestState.Failed,
                Message = message.Exceptions.PrimaryException.DisplayName,
                StackTrace = message.Exceptions.CompoundStackTrace,
            });
        }

        public void AssemblyCompleted(AssemblyInfo message, AssemblyResult result)
        {
        }
    }
}