namespace Fixie.Tests.Internal
{
    using System.Collections.Generic;
    using Assertions;
    using Fixie.Internal;
    using static Utility;

    public class ExecutionSummaryTests
    {
        public void ShouldAccumulateCaseStatusCounts()
        {
            var discovery = new SelfTestDiscovery();
            var execution = new CreateInstancePerCase();

            var listener = new StubExecutionSummaryListener();

            RunTypes(listener, discovery, execution, typeof(FirstSampleTestClass), typeof(SecondSampleTestClass));

            listener.ClassSummaries.Count.ShouldBe(2);
            listener.AssemblySummary.Count.ShouldBe(1);

            var classA = listener.ClassSummaries[0];
            var classB = listener.ClassSummaries[1];
            var assembly = listener.AssemblySummary[0];

            classA.Passed.ShouldBe(1);
            classA.Failed.ShouldBe(1);
            classA.Skipped.ShouldBe(1);
            classA.Total.ShouldBe(3);

            classB.Passed.ShouldBe(1);
            classB.Failed.ShouldBe(2);
            classB.Skipped.ShouldBe(3);
            classB.Total.ShouldBe(6);

            assembly.Passed.ShouldBe(2);
            assembly.Failed.ShouldBe(3);
            assembly.Skipped.ShouldBe(4);
            assembly.Total.ShouldBe(9);
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

        class CreateInstancePerCase : Execution
        {
            public void Execute(TestClass testClass)
            {
                testClass.RunCases(@case =>
                {
                    if (@case.Method.Name.Contains("Skip"))
                        return;

                    @case.Execute(testClass.Construct());
                });
            }
        }
    }
}