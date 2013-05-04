using System;
using System.Collections.Generic;
using System.Linq;
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

        public void CaseFailed(Case @case, Exception[] exceptions)
        {
            tdnet.TestFinished(new TestResult
            {
                Name = @case.Name,
                State = TestState.Failed,
                Message = exceptions.First().Message,
                StackTrace = GetCompoundStackTrace(exceptions),
            });
        }

        static string GetCompoundStackTrace(IEnumerable<Exception> exceptions)
        {
            var stackTrace = new StringBuilder();

            bool isPrimaryException = true;

            foreach (var ex in exceptions)
            {
                if (isPrimaryException)
                {
                    stackTrace.Append(ex.StackTrace);
                }
                else
                {
                    stackTrace.AppendLine();
                    stackTrace.AppendLine();
                    stackTrace.AppendLine("===== Secondary Exception =====");
                    stackTrace.AppendLine(ex.GetType().FullName);
                    stackTrace.AppendLine(ex.Message);
                    stackTrace.Append(ex.StackTrace);
                }

                var walk = ex;
                while (walk.InnerException != null)
                {
                    walk = walk.InnerException;
                    stackTrace.AppendLine();
                    stackTrace.AppendLine();
                    stackTrace.AppendLine("----- Inner Exception -----");

                    stackTrace.AppendLine(walk.GetType().FullName);
                    stackTrace.AppendLine(walk.Message);
                    stackTrace.Append(walk.StackTrace);
                }

                isPrimaryException = false;
            }

            return stackTrace.ToString();
        }

        public void RunComplete(Result result)
        {
        }
    }
}