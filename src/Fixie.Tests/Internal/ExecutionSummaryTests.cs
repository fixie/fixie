﻿namespace Fixie.Tests.Internal;

using System.Collections.Generic;
using System.Threading.Tasks;
using Assertions;
using Fixie.Reports;
using static Utility;

public class ExecutionSummaryTests
{
    public async Task ShouldAccumulateTestResultCounts()
    {
        var report = new StubExecutionSummaryReport();
        var discovery = new SelfTestDiscovery();
        var execution = new CreateInstancePerCase();

        await Run(report, discovery, execution, typeof(FirstSampleTestClass), typeof(SecondSampleTestClass));

        report.ExecutionCompletions.Count.ShouldBe(1);

        var executionCompleted = report.ExecutionCompletions[0];

        executionCompleted.Passed.ShouldBe(2);
        executionCompleted.Failed.ShouldBe(3);
        executionCompleted.Skipped.ShouldBe(4);
        executionCompleted.Total.ShouldBe(9);
    }

    class StubExecutionSummaryReport :
        IHandler<ExecutionCompleted>
    {
        public List<ExecutionCompleted> ExecutionCompletions { get; } = new List<ExecutionCompleted>();

        public Task Handle(ExecutionCompleted message)
        {
            ExecutionCompletions.Add(message);
            return Task.CompletedTask;
        }
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

    class CreateInstancePerCase : IExecution
    {
        public async Task Run(TestSuite testSuite)
        {
            foreach (var test in testSuite.Tests)
                if (!test.Name.Contains("Skip"))
                    await test.Run();
        }
    }
}