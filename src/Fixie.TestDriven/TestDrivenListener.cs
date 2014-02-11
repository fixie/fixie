using System.Reflection;
using Fixie.Results;
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

        public void AssemblyStarted(Assembly assembly)
        {
        }

        public void CaseSkipped(SkipResult result)
        {
            tdnet.TestFinished(new TestResult
            {
                Name = result.Case.Name,
                State = TestState.Ignored
            });
        }

        public void CasePassed(PassResult result)
        {
            var @case = result.Case;
            tdnet.TestFinished(new TestResult
            {
                Name = @case.Name,
                State = TestState.Passed
            });
        }

        public void CaseFailed(FailResult result)
        {
            var @case = result.Case;

            tdnet.TestFinished(new TestResult
            {
                Name = @case.Name,
                State = TestState.Failed,
                Message = result.PrimaryExceptionTypeName(),
                StackTrace = result.CompoundStackTrace(),
            });
        }

        public void AssemblyCompleted(Assembly assembly, AssemblyResult result)
        {
        }
    }
}