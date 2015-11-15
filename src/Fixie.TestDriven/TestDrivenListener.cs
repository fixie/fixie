using Fixie.Execution;
using TestDriven.Framework;

namespace Fixie.TestDriven
{
    public class TestDrivenListener : Listener,
        IHandler<AssemblyStarted>,
        IHandler<SkipResult>,
        IHandler<PassResult>,
        IHandler<FailResult>,
        IHandler<AssemblyCompleted>
    {
        readonly ITestListener tdnet;

        public TestDrivenListener(ITestListener tdnet)
        {
            this.tdnet = tdnet;
        }

        public void Handle(AssemblyStarted message)
        {
        }

        public void Handle(SkipResult result)
        {
            tdnet.TestFinished(new TestResult
            {
                Name = result.Name,
                State = TestState.Ignored,
                Message = result.SkipReason
            });
        }

        public void Handle(PassResult result)
        {
            tdnet.TestFinished(new TestResult
            {
                Name = result.Name,
                State = TestState.Passed
            });
        }

        public void Handle(FailResult result)
        {
            tdnet.TestFinished(new TestResult
            {
                Name = result.Name,
                State = TestState.Failed,
                Message = result.Exceptions.PrimaryException.DisplayName,
                StackTrace = result.Exceptions.CompoundStackTrace,
            });
        }

        public void Handle(AssemblyCompleted message)
        {
        }
    }
}