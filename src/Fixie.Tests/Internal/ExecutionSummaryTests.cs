namespace Fixie.Tests.Internal
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Assertions;
    using Fixie.Reports;
    using static Utility;

    public class ExecutionSummaryTests
    {
        public async Task ShouldAccumulateCaseStatusCounts()
        {
            var discovery = new SelfTestDiscovery();
            var execution = new CreateInstancePerCase();

            var report = new StubExecutionSummaryReport();

            await RunAsync(report, discovery, execution, typeof(FirstSampleTestClass), typeof(SecondSampleTestClass));

            report.AssemblySummary.Count.ShouldBe(1);

            var assembly = report.AssemblySummary[0];

            assembly.Passed.ShouldBe(2);
            assembly.Failed.ShouldBe(3);
            assembly.Skipped.ShouldBe(4);
            assembly.Total.ShouldBe(9);
        }

        class StubExecutionSummaryReport :
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
            public async Task RunAsync(TestClass testClass)
            {
                foreach (var test in testClass.Tests)
                    if (!test.Method.Name.Contains("Skip"))
                        await test.RunAsync();
            }
        }
    }
}