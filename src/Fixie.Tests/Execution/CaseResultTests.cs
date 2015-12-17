using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Fixie.Execution;
using Fixie.Internal;
using Should;

namespace Fixie.Tests.Execution
{
    public class CaseResultTests
    {
        public void ShouldDescribeCaseResults()
        {
            var convention = SelfTestConvention.Build();
            convention.CaseExecution.Skip(x => x.Method.Name == "Skip", x => "Skipped by naming convention.");

            using (new RedirectedConsole())
            {
                var assemblyResult = typeof(SampleTestClass).Run(convention);
                var classResult = assemblyResult.ClassResults.Single();

                var skip = classResult.CaseResults[0];
                var fail = classResult.CaseResults[1];
                var pass = classResult.CaseResults[2];

                pass.Name.ShouldEqual("Fixie.Tests.Execution.CaseResultTests+SampleTestClass.Pass");
                pass.MethodGroup.FullName.ShouldEqual("Fixie.Tests.Execution.CaseResultTests+SampleTestClass.Pass");
                pass.Output.ShouldEqual("Pass" + Environment.NewLine);
                pass.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);
                pass.Status.ShouldEqual(CaseStatus.Passed);
                pass.ExceptionSummary.ShouldBeNull();
                pass.Message.ShouldBeNull();

                fail.Name.ShouldEqual("Fixie.Tests.Execution.CaseResultTests+SampleTestClass.Fail");
                fail.MethodGroup.FullName.ShouldEqual("Fixie.Tests.Execution.CaseResultTests+SampleTestClass.Fail");
                fail.Output.ShouldEqual("Fail" + Environment.NewLine);
                fail.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);
                fail.Status.ShouldEqual(CaseStatus.Failed);
                fail.ExceptionSummary.ShouldEqual("Fixie.Tests.FailureException");
                fail.Message.ShouldEqual("'Fail' failed!");

                skip.Name.ShouldEqual("Fixie.Tests.Execution.CaseResultTests+SampleTestClass.Skip");
                skip.MethodGroup.FullName.ShouldEqual("Fixie.Tests.Execution.CaseResultTests+SampleTestClass.Skip");
                skip.Output.ShouldBeNull();
                skip.Duration.ShouldEqual(TimeSpan.Zero);
                skip.Status.ShouldEqual(CaseStatus.Skipped);
                skip.ExceptionSummary.ShouldBeNull();
                skip.Message.ShouldEqual("Skipped by naming convention.");
            }
        }

        static void WhereAmI([CallerMemberName] string member = null)
        {
            Console.WriteLine(member);
        }

        class SampleTestClass
        {
            public void Fail()
            {
                WhereAmI();
                throw new FailureException();
            }

            public void Pass()
            {
                WhereAmI();
            }

            public void Skip()
            {
                WhereAmI();
            }
        }
    }
}
