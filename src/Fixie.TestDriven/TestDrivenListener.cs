using System;
using TestDriven.Framework;

namespace Fixie.TestDriven
{
    public class TestDrivenListener : Listener
    {
        readonly ITestListener tdnet;
        readonly RunState runState = new RunState();

        public TestDrivenListener(ITestListener tdnet)
        {
            this.tdnet = tdnet;
        }

        public void CasePassed(Case @case)
        {
            runState.CasePassed();
            tdnet.TestFinished(new TestResult
            {
                Name = @case.Name,
                State = TestState.Passed
            });
        }

        public void CaseFailed(Case @case, Exception ex)
        {
            runState.CaseFailed();
            tdnet.TestFinished(new TestResult
            {
                Name = @case.Name,
                State = TestState.Failed,
                Message = ex.Message,
                StackTrace = ex.StackTrace,
            });
        }

        public void AssemblyComplete()
        {
        }

        public RunState State { get { return runState; } }
    }
}