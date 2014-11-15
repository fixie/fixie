using System.Reflection;
using Fixie.Execution;
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

        public void AssemblyStarted(string assemblyFileName)
        {
        }

        public void CaseSkipped(SkipResult result)
        {
            tdnet.TestFinished(new TestResult
            {
                Name = result.Name,
                State = TestState.Ignored
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

        public void AssemblyCompleted(string assemblyFileName, AssemblyResult result)
        {
        }
    }
}