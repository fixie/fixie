using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Fixie.Execution;
using Fixie.Internal;
using Should;
using Should.Core.Exceptions;

namespace Fixie.Tests.Execution
{
    public class LifecycleMessageTests
    {
        public void ShouldDescribeCaseCompletion()
        {
            var convention = SelfTestConvention.Build();
            convention.CaseExecution.Skip(x => x.Method.Name == "Skip");
            convention.CaseExecution.Skip(x => x.Method.Name == "SkipWithReason", x => "Skipped by naming convention.");
            convention.HideExceptionDetails.For<EqualException>();

            var listener = new StubCaseCompletedListener();

            using (new RedirectedConsole())
            {
                typeof(SampleTestClass).Run(listener, convention);

                var assembly = typeof(LifecycleMessageTests).Assembly;

                var assemblyStarted = listener.AssemblyStarts.Single();
                assemblyStarted.Name.ShouldEqual("Fixie.Tests");
                assemblyStarted.Location.ShouldEqual(assembly.Location);

                listener.Cases.Count.ShouldEqual(5);

                var skip = listener.Cases[0];
                var skipWithReason = listener.Cases[1];
                var fail = listener.Cases[2];
                var failByAssertion = listener.Cases[3];
                var pass = listener.Cases[4];

                pass.Name.ShouldEqual("Fixie.Tests.Execution.LifecycleMessageTests+SampleTestClass.Pass");
                pass.MethodGroup.FullName.ShouldEqual("Fixie.Tests.Execution.LifecycleMessageTests+SampleTestClass.Pass");
                pass.Output.ShouldEqual("Pass" + Environment.NewLine);
                pass.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);
                pass.Status.ShouldEqual(CaseStatus.Passed);
                pass.Exceptions.ShouldBeNull();
                pass.SkipReason.ShouldBeNull();

                fail.Name.ShouldEqual("Fixie.Tests.Execution.LifecycleMessageTests+SampleTestClass.Fail");
                fail.MethodGroup.FullName.ShouldEqual("Fixie.Tests.Execution.LifecycleMessageTests+SampleTestClass.Fail");
                fail.Output.ShouldEqual("Fail" + Environment.NewLine);
                fail.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);
                fail.Status.ShouldEqual(CaseStatus.Failed);
                fail.Exceptions.Type.ShouldEqual("Fixie.Tests.FailureException");
                fail.Exceptions.StackTrace.ShouldNotBeNull();
                fail.Exceptions.Message.ShouldEqual("'Fail' failed!");
                fail.SkipReason.ShouldBeNull();

                failByAssertion.Name.ShouldEqual("Fixie.Tests.Execution.LifecycleMessageTests+SampleTestClass.FailByAssertion");
                failByAssertion.MethodGroup.FullName.ShouldEqual("Fixie.Tests.Execution.LifecycleMessageTests+SampleTestClass.FailByAssertion");
                failByAssertion.Output.ShouldEqual("FailByAssertion" + Environment.NewLine);
                failByAssertion.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);
                failByAssertion.Status.ShouldEqual(CaseStatus.Failed);
                failByAssertion.Exceptions.Type.ShouldEqual("Should.Core.Exceptions.EqualException");
                failByAssertion.Exceptions.StackTrace.ShouldNotBeNull();
                failByAssertion.Exceptions.Message.Lines().ShouldEqual(
                    "Assert.Equal() Failure",
                    "Expected: 2",
                    "Actual:   1");
                failByAssertion.SkipReason.ShouldBeNull();

                skip.Name.ShouldEqual("Fixie.Tests.Execution.LifecycleMessageTests+SampleTestClass.Skip");
                skip.MethodGroup.FullName.ShouldEqual("Fixie.Tests.Execution.LifecycleMessageTests+SampleTestClass.Skip");
                skip.Output.ShouldBeNull();
                skip.Duration.ShouldEqual(TimeSpan.Zero);
                skip.Status.ShouldEqual(CaseStatus.Skipped);
                skip.Exceptions.ShouldBeNull();
                skip.SkipReason.ShouldBeNull();

                skipWithReason.Name.ShouldEqual("Fixie.Tests.Execution.LifecycleMessageTests+SampleTestClass.SkipWithReason");
                skipWithReason.MethodGroup.FullName.ShouldEqual("Fixie.Tests.Execution.LifecycleMessageTests+SampleTestClass.SkipWithReason");
                skipWithReason.Output.ShouldBeNull();
                skipWithReason.Duration.ShouldEqual(TimeSpan.Zero);
                skipWithReason.Status.ShouldEqual(CaseStatus.Skipped);
                skipWithReason.Exceptions.ShouldBeNull();
                skipWithReason.SkipReason.ShouldEqual("Skipped by naming convention.");

                var assemblyCompleted = listener.AssemblyCompletions.Single();
                assemblyCompleted.Name.ShouldEqual("Fixie.Tests");
                assemblyCompleted.Location.ShouldEqual(assembly.Location);
            }
        }

        public class StubCaseCompletedListener :
            Handler<AssemblyStarted>,
            Handler<CaseCompleted>,
            Handler<AssemblyCompleted> 
        {
            public List<AssemblyStarted> AssemblyStarts { get; set; } = new List<AssemblyStarted>();
            public List<CaseCompleted> Cases { get; set; } = new List<CaseCompleted>();
            public List<AssemblyCompleted> AssemblyCompletions { get; set; } = new List<AssemblyCompleted>();

            public void Handle(AssemblyStarted message) => AssemblyStarts.Add(message);
            public void Handle(CaseCompleted message) => Cases.Add(message);
            public void Handle(AssemblyCompleted message) => AssemblyCompletions.Add(message);
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
