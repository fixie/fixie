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

            Run(listener, discovery, execution, typeof(FirstSampleTestClass), typeof(SecondSampleTestClass));

            listener.AssemblySummary.Count.ShouldBe(1);

            var assembly = listener.AssemblySummary[0];

            assembly.Passed.ShouldBe(2);
            assembly.Failed.ShouldBe(3);
            assembly.Skipped.ShouldBe(4);
            assembly.Total.ShouldBe(9);
        }

        class StubExecutionSummaryListener :
            Handler<AssemblyCompleted>
        {
            public List<AssemblyCompleted> AssemblySummary { get; } = new List<AssemblyCompleted>();

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
                foreach (var test in testClass.Tests)
                    if (!test.Method.Name.Contains("Skip"))
                        test.Run().GetAwaiter().GetResult();
            }
        }
    }
}