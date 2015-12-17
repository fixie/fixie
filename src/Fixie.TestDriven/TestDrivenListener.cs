using System;
using Fixie.Execution;
using TestDriven.Framework;

namespace Fixie.TestDriven
{
    public class TestDrivenListener : IHandler<CaseResult>
    {
        readonly ITestListener tdnet;

        public TestDrivenListener(ITestListener tdnet)
        {
            this.tdnet = tdnet;
        }

        public void Handle(CaseResult message)
        {
            tdnet.TestFinished(Map(message));
        }

        static TestResult Map(CaseResult message)
        {
            var testResult = new TestResult
            {
                Name = message.Name,
                State = Map(message.Status)
            };

            if (message.Status == CaseStatus.Failed)
            {
                testResult.Message = message.IsAssertionException ? "" : message.ExceptionType;
                testResult.StackTrace = message.StackTrace;
            }
            else if (message.Status == CaseStatus.Skipped)
            {
                testResult.Message = message.Message;
            }

            return testResult;
        }

        static TestState Map(CaseStatus caseStatus)
        {
            switch (caseStatus)
            {
                case CaseStatus.Passed:
                    return TestState.Passed;
                case CaseStatus.Failed:
                    return TestState.Failed;
                case CaseStatus.Skipped:
                    return TestState.Ignored;
                default:
                    throw new ArgumentOutOfRangeException(nameof(caseStatus));
            }
        }
    }
}