using System;
using System.Reflection;
using System.Text;
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
            var firstMessage = ex.Message;
            var stackTrace = new StringBuilder();
            stackTrace.Append(ex.StackTrace);

            while (ex.InnerException != null)
            {
                ex = ex.InnerException;
                stackTrace.AppendLine();
                stackTrace.AppendLine();
                stackTrace.AppendLine("----- Inner Exception -----");

                stackTrace.AppendLine(ex.GetType().FullName);
                stackTrace.AppendLine(ex.Message);
                stackTrace.Append(ex.StackTrace);
            }

            tdnet.TestFinished(new TestResult
            {
                Name = @case.Name,
                State = TestState.Failed,
                Message = firstMessage,
                StackTrace = stackTrace.ToString(),
            });
        }

        public void RunComplete(Result result)
        {
        }
    }
}