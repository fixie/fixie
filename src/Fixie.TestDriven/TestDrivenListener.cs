using System;
using System.Reflection;
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

        public void RunStarted(Assembly context)
        {
        }

        public void CasePassed(Case @case)
        {
            tdnet.TestFinished(new TestResult
            {
                Name = @case.Name,
                State = TestState.Passed
            });
        }

        public void CaseFailed(Case @case, Exception ex)
        {
            tdnet.TestFinished(new TestResult
            {
                Name = @case.Name,
                State = TestState.Failed,
                Message = ex.Message,
                StackTrace = ex.StackTrace,
            });
        }

        public void RunComplete(Result result)
        {
        }
    }
}