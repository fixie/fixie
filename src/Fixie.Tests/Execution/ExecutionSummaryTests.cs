using System;
using Fixie.Execution;
using Should;

namespace Fixie.Tests.Execution
{
    public class ExecutionSummaryTests
    {
        public void ShouldAccumulateCaseStatusCountsAndDurations()
        {
            var convention = SelfTestConvention.Build();
            convention.CaseExecution.Skip(x => x.Method.Name.StartsWith("Skip"));

            var listener = new StubExecutionSummaryListener();

            typeof(SampleTestClass).Run(listener, convention);

            var summary = listener.Summary;

            summary.Passed.ShouldEqual(1);
            summary.Failed.ShouldEqual(2);
            summary.Skipped.ShouldEqual(3);
            summary.Total.ShouldEqual(6);
            summary.Duration.ShouldEqual(listener.ExpectedDuration);
        }

        class StubExecutionSummaryListener : Handler<CaseCompleted>
        {
            public TimeSpan ExpectedDuration { get; private set; }
            public ExecutionSummary Summary { get; } = new ExecutionSummary();

            public void Handle(CaseCompleted message)
            {
                ExpectedDuration += message.Duration;
                Summary.Add(message);
            }
        }

        class SampleTestClass
        {
            public void Pass() { }
            public void FailA() { throw new FailureException(); }
            public void FailB() { throw new FailureException(); }
            public void SkipA() { }
            public void SkipB() { }
            public void SkipC() { }
        }
    }
}
