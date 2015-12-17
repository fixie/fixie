using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Fixie.Execution;
using Fixie.Internal;
using Should;
using Should.Core.Exceptions;

namespace Fixie.Tests.Execution
{
    public class CaseResultTests
    {
        public void ShouldDescribeCaseResults()
        {
            var convention = SelfTestConvention.Build();
            convention.CaseExecution.Skip(x => x.Method.Name == "Skip");
            convention.CaseExecution.Skip(x => x.Method.Name == "SkipWithReason", x => "Skipped by naming convention.");
            convention.HideExceptionDetails.For<EqualException>();

            using (new RedirectedConsole())
            {
                var assemblyResult = typeof(SampleTestClass).Run(convention);
                var classResult = assemblyResult.ClassResults.Single();

                classResult.CaseResults.Count.ShouldEqual(5);

                var skip = classResult.CaseResults[0];
                var skipWithReason = classResult.CaseResults[1];
                var fail = classResult.CaseResults[2];
                var failByAssertion = classResult.CaseResults[3];
                var pass = classResult.CaseResults[4];

                pass.Name.ShouldEqual("Fixie.Tests.Execution.CaseResultTests+SampleTestClass.Pass");
                pass.MethodGroup.FullName.ShouldEqual("Fixie.Tests.Execution.CaseResultTests+SampleTestClass.Pass");
                pass.Output.ShouldEqual("Pass" + Environment.NewLine);
                pass.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);
                pass.Status.ShouldEqual(CaseStatus.Passed);
                pass.IsAssertionException.ShouldBeFalse();
                pass.ExceptionType.ShouldBeNull();
                pass.StackTrace.ShouldBeNull();
                pass.Message.ShouldBeNull();

                fail.Name.ShouldEqual("Fixie.Tests.Execution.CaseResultTests+SampleTestClass.Fail");
                fail.MethodGroup.FullName.ShouldEqual("Fixie.Tests.Execution.CaseResultTests+SampleTestClass.Fail");
                fail.Output.ShouldEqual("Fail" + Environment.NewLine);
                fail.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);
                fail.Status.ShouldEqual(CaseStatus.Failed);
                fail.IsAssertionException.ShouldBeFalse();
                fail.ExceptionType.ShouldEqual("Fixie.Tests.FailureException");
                fail.StackTrace.ShouldNotBeNull();
                fail.Message.ShouldEqual("'Fail' failed!");

                failByAssertion.Name.ShouldEqual("Fixie.Tests.Execution.CaseResultTests+SampleTestClass.FailByAssertion");
                failByAssertion.MethodGroup.FullName.ShouldEqual("Fixie.Tests.Execution.CaseResultTests+SampleTestClass.FailByAssertion");
                failByAssertion.Output.ShouldEqual("FailByAssertion" + Environment.NewLine);
                failByAssertion.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);
                failByAssertion.Status.ShouldEqual(CaseStatus.Failed);
                failByAssertion.IsAssertionException.ShouldBeTrue();
                failByAssertion.ExceptionType.ShouldEqual("Should.Core.Exceptions.EqualException");
                failByAssertion.StackTrace.ShouldNotBeNull();
                failByAssertion.Message.Lines().ShouldEqual(
                    "Assert.Equal() Failure",
                    "Expected: 2",
                    "Actual:   1");

                skip.Name.ShouldEqual("Fixie.Tests.Execution.CaseResultTests+SampleTestClass.Skip");
                skip.MethodGroup.FullName.ShouldEqual("Fixie.Tests.Execution.CaseResultTests+SampleTestClass.Skip");
                skip.Output.ShouldBeNull();
                skip.Duration.ShouldEqual(TimeSpan.Zero);
                skip.Status.ShouldEqual(CaseStatus.Skipped);
                skip.IsAssertionException.ShouldBeFalse();
                skip.ExceptionType.ShouldBeNull();
                skip.StackTrace.ShouldBeNull();
                skip.Message.ShouldBeNull();

                skipWithReason.Name.ShouldEqual("Fixie.Tests.Execution.CaseResultTests+SampleTestClass.SkipWithReason");
                skipWithReason.MethodGroup.FullName.ShouldEqual("Fixie.Tests.Execution.CaseResultTests+SampleTestClass.SkipWithReason");
                skipWithReason.Output.ShouldBeNull();
                skipWithReason.Duration.ShouldEqual(TimeSpan.Zero);
                skipWithReason.Status.ShouldEqual(CaseStatus.Skipped);
                skipWithReason.IsAssertionException.ShouldBeFalse();
                skipWithReason.ExceptionType.ShouldBeNull();
                skipWithReason.StackTrace.ShouldBeNull();
                skipWithReason.Message.ShouldEqual("Skipped by naming convention.");
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

            public void FailByAssertion()
            {
                WhereAmI();
                1.ShouldEqual(2);
            }

            public void Pass()
            {
                WhereAmI();
            }

            public void Skip()
            {
                WhereAmI();
            }

            public void SkipWithReason()
            {
                WhereAmI();
            }
        }
    }
}
