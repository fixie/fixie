namespace Fixie.Tests.Execution
{
    using Should;
    using System;
    using Fixie.Execution;

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

        public void ShouldProvideUserFacingStringRepresentation()
        {
            var convention = SelfTestConvention.Build();
            convention.CaseExecution.Skip(x => x.Method.Name.StartsWith("Skip"));

            var listener = new StubExecutionSummaryListener();

            typeof(SampleTestClass).Run(listener, convention);

            listener.Summary
                .ToString()
                .CleanDuration()
                .ShouldEqual("1 passed, 2 failed, 3 skipped, took 1.23 seconds");
        }

        public void ShouldNotReportSkipCountsInUserFacingStringRepresentationWhenZeroTestsHaveBeenSkipped()
        {
            var convention = SelfTestConvention.Build();

            convention
                .Methods
                .Where(method => !method.Name.StartsWith("Skip"));

            var listener = new StubExecutionSummaryListener();

            typeof(SampleTestClass).Run(listener, convention);

            listener.Summary
                .ToString()
                .CleanDuration()
                .ShouldEqual("1 passed, 2 failed, took 1.23 seconds");
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