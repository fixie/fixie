namespace Fixie.Tests.Execution
{
    using System;
    using System.Collections.Generic;
    using Assertions;
    using Fixie.Execution;
    using static Utility;

    public class ExecutionSummaryTests
    {
        public void ShouldAccumulateCaseStatusCounts()
        {
            var convention = SelfTestConvention.Build();
            convention.ClassExecution.Lifecycle<CreateInstancePerCase>();

            var listener = new StubExecutionSummaryListener();

            RunTypes(listener, convention, typeof(FirstSampleTestClass), typeof(SecondSampleTestClass));

            listener.ClassSummaries.Count.ShouldEqual(2);
            listener.AssemblySummary.Count.ShouldEqual(1);

            var classA = listener.ClassSummaries[0];
            var classB = listener.ClassSummaries[1];
            var assembly = listener.AssemblySummary[0];

            classA.Passed.ShouldEqual(1);
            classA.Failed.ShouldEqual(1);
            classA.Skipped.ShouldEqual(1);
            classA.Total.ShouldEqual(3);

            classB.Passed.ShouldEqual(1);
            classB.Failed.ShouldEqual(2);
            classB.Skipped.ShouldEqual(3);
            classB.Total.ShouldEqual(6);

            assembly.Passed.ShouldEqual(2);
            assembly.Failed.ShouldEqual(3);
            assembly.Skipped.ShouldEqual(4);
            assembly.Total.ShouldEqual(9);
        }

        class StubExecutionSummaryListener :
            Handler<ClassCompleted>,
            Handler<AssemblyCompleted>
        {
            public List<ClassCompleted> ClassSummaries { get; } = new List<ClassCompleted>();
            public List<AssemblyCompleted> AssemblySummary { get; } = new List<AssemblyCompleted>();

            public void Handle(ClassCompleted message) => ClassSummaries.Add(message);
            public void Handle(AssemblyCompleted message) => AssemblySummary.Add(message);
        }

        class FirstSampleTestClass
        {
            public void Pass() { }
            public void Fail() { throw new FailureException(); }
            public void Skip() { }
        }

        class SecondSampleTestClass
        {
            public void Pass() { }
            public void FailA() { throw new FailureException(); }
            public void FailB() { throw new FailureException(); }
            public void SkipA() { }
            public void SkipB() { }
            public void SkipC() { }
        }

        class CreateInstancePerCase : Lifecycle
        {
            public void Execute(TestClass testClass, Action<CaseAction> runCases)
            {
                runCases(@case =>
                {
                    if (@case.Method.Name.Contains("Skip"))
                        return;

                    @case.Execute(testClass.Construct());
                });
            }
        }
    }
}