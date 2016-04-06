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

        public void Handle(AssemblyInfo message)
        {
        }

        public void Handle(SkipResult message)
        {
            tdnet.TestFinished(new TestResult
            {
                Name = message.Name,
                State = TestState.Ignored,
                Message = message.SkipReason
            });
        }

        public void Handle(PassResult message)
        {
            tdnet.TestFinished(new TestResult
            {
                Name = message.Name,
                State = TestState.Passed
            });
        }

        public void Handle(FailResult message)
        {
            tdnet.TestFinished(new TestResult
            {
                Name = message.Name,
                State = TestState.Failed,
                Message = message.Exceptions.PrimaryException.DisplayName,
                StackTrace = message.Exceptions.CompoundStackTrace,
            });
        }

        public void Handle(AssemblyCompleted message)
        {
        }
    }
}