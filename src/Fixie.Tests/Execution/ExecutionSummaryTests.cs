namespace Fixie.Tests.Execution
{
    using System;
    using Assertions;
    using Fixie.Execution;
    using static Utility;

    public class ExecutionSummaryTests
    {
        public void ShouldAccumulateCaseStatusCountsAndDurations()
        {
            var convention = SelfTestConvention.Build();
            convention.CaseExecution.Skip(x => x.Method.Name.StartsWith("Skip"));

            var listener = new StubExecutionSummaryListener();

            Run<SampleTestClass>(listener, convention);

            var summary = listener.SummaryOfCases;

            summary.Passed.ShouldEqual(1);
            summary.Failed.ShouldEqual(2);
            summary.Skipped.ShouldEqual(3);
            summary.Total.ShouldEqual(6);
            summary.Duration.ShouldEqual(listener.ExpectedDuration);

            var summaryOfAssembly = listener.SummaryOfAssembly;

            summaryOfAssembly.Passed.ShouldEqual(1);
            summaryOfAssembly.Failed.ShouldEqual(2);
            summaryOfAssembly.Skipped.ShouldEqual(3);
            summaryOfAssembly.Total.ShouldEqual(6);
            summaryOfAssembly.Duration.ShouldEqual(listener.ExpectedDuration);
        }

        public void ShouldProvideUserFacingStringRepresentation()
        {
            var convention = SelfTestConvention.Build();
            convention.CaseExecution.Skip(x => x.Method.Name.StartsWith("Skip"));

            var listener = new StubExecutionSummaryListener();

            Run<SampleTestClass>(listener, convention);

            listener.SummaryOfCases
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

            Run<SampleTestClass>(listener, convention);

            listener.SummaryOfCases
                .ToString()
                .CleanDuration()
                .ShouldEqual("1 passed, 2 failed, took 1.23 seconds");
        }

        class StubExecutionSummaryListener : Handler<CaseCompleted>, Handler<AssemblyCompleted>
        {
            public TimeSpan ExpectedDuration { get; private set; }
            public ExecutionSummary SummaryOfCases { get; } = new ExecutionSummary();
            public ExecutionSummary SummaryOfAssembly { get; private set; }

            public void Handle(CaseCompleted message)
            {
                ExpectedDuration += message.Duration;
                SummaryOfCases.Add(message);
            }

            public void Handle(AssemblyCompleted message)
            {
                SummaryOfAssembly = message.Summary;
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