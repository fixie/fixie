using Fixie.Execution;
using TestDriven.Framework;

namespace Fixie.TestDriven
{
    public class TestDrivenListener : Listener
    {
        readonly ITestListener tdnet;

        public TestDrivenListener(ITestListener tdnet)
        {
            this.tdnet = tdnet;
        }

        public void AssemblyStarted(AssemblyInfo assembly)
        {
        }

        public void CaseSkipped(SkipResult result)
        {
            tdnet.TestFinished(new TestResult
            {
                Name = result.Name,
                State = TestState.Ignored,
                Message = result.SkipReason
            });
        }

        public void CasePassed(PassResult result)
        {
            tdnet.TestFinished(new TestResult
            {
                Name = result.Name,
                State = TestState.Passed
            });
        }

        public void CaseFailed(FailResult result)
        {
            tdnet.TestFinished(new TestResult
            {
                Name = result.Name,
                State = TestState.Failed,
                Message = result.Exceptions.PrimaryException.DisplayName,
                StackTrace = result.Exceptions.CompoundStackTrace,
            });
        }

        public void AssemblyCompleted(AssemblyCompleted message)
        {
        }
    }
}