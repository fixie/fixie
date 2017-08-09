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

            var @class = listener.SummaryOfClass;

            @class.Passed.ShouldEqual(1);
            @class.Failed.ShouldEqual(2);
            @class.Skipped.ShouldEqual(3);
            @class.Total.ShouldEqual(6);
            @class.Duration.ShouldEqual(listener.ExpectedDuration);

            var assembly = listener.SummaryOfAssembly;

            assembly.Passed.ShouldEqual(1);
            assembly.Failed.ShouldEqual(2);
            assembly.Skipped.ShouldEqual(3);
            assembly.Total.ShouldEqual(6);
            assembly.Duration.ShouldEqual(listener.ExpectedDuration);
        }

        public void ShouldProvideUserFacingStringRepresentation()
        {
            var convention = SelfTestConvention.Build();
            convention.CaseExecution.Skip(x => x.Method.Name.StartsWith("Skip"));

            var listener = new StubExecutionSummaryListener();

            Run<SampleTestClass>(listener, convention);

            listener.SummaryOfAssembly
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

            listener.SummaryOfAssembly
                .ToString()
                .CleanDuration()
                .ShouldEqual("1 passed, 2 failed, took 1.23 seconds");
        }

        class StubExecutionSummaryListener :
            Handler<CaseCompleted>,
            Handler<ClassCompleted>,
            Handler<AssemblyCompleted>
        {
            public TimeSpan ExpectedDuration { get; private set; }
            public ExecutionSummary SummaryOfClass { get; private set; }
            public ExecutionSummary SummaryOfAssembly { get; private set; }

            public void Handle(CaseCompleted message)
            {
                ExpectedDuration += message.Duration;
            }

            public void Handle(ClassCompleted message)
            {
                SummaryOfClass = message.Summary;
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