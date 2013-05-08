using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public void CaseFailed(Case @case, Exception[] exceptions)
        {
            tdnet.TestFinished(new TestResult
            {
                Name = @case.Name,
                State = TestState.Failed,
                Message = exceptions.First().GetType().FullName,
                StackTrace = CompoundStackTrace(exceptions),
            });
        }

        static string CompoundStackTrace(IEnumerable<Exception> exceptions)
        {
            using (var writer = new StringWriter())
            {
                writer.WriteCompoundStackTrace(exceptions);
                return writer.ToString();
            }
        }

        public void RunComplete(Result result)
        {
        }
    }
}