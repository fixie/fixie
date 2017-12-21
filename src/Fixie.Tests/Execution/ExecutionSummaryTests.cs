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
            var listener = Run();

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
            Run()
                .SummaryOfAssembly
                .ToString()
                .CleanDuration()
                .ShouldEqual("1 passed, 2 failed, 3 skipped, took 1.23 seconds");
        }

        public void ShouldNotReportPassCountsInUserFacingStringRepresentationWhenZeroTestsHavePassed()
        {
            void ZeroPassed(Convention convention)
                => convention.Methods.Where(method => !method.Name.StartsWith("Pass"));

            Run(ZeroPassed)
                .SummaryOfAssembly
                .ToString()
                .CleanDuration()
                .ShouldEqual("2 failed, 3 skipped, took 1.23 seconds");
        }

        public void ShouldNotReportFailCountsInUserFacingStringRepresentationWhenZeroTestsHaveFailed()
        {
            void ZeroFailed(Convention convention)
                => convention.Methods.Where(method => !method.Name.StartsWith("Fail"));

            Run(ZeroFailed)
                .SummaryOfAssembly
                .ToString()
                .CleanDuration()
                .ShouldEqual("1 passed, 3 skipped, took 1.23 seconds");
        }

        public void ShouldNotReportSkipCountsInUserFacingStringRepresentationWhenZeroTestsHaveBeenSkipped()
        {
            void ZeroSkipped(Convention convention)
                => convention.Methods.Where(method => !method.Name.StartsWith("Skip"));

            Run(ZeroSkipped)
                .SummaryOfAssembly
                .ToString()
                .CleanDuration()
                .ShouldEqual("1 passed, 2 failed, took 1.23 seconds");
        }

        public void ShouldProvideDiagnosticUserFacingStringRepresentationWhenNoTestsWereExecuted()
        {
            void NoTestsFound(Convention convention)
                => convention.Methods.Where(method => false);

            Run(NoTestsFound)
                .SummaryOfAssembly
                .ToString()
                .CleanDuration()
                .ShouldEqual("No tests found.");
        }

        static StubExecutionSummaryListener Run(Action<Convention> customize = null)
        {
            var convention = SelfTestConvention.Build();
            convention.CaseExecution.Skip(x => x.Method.Name.StartsWith("Skip"));

            customize?.Invoke(convention);

            var listener = new StubExecutionSummaryListener();

            Run<SampleTestClass>(listener, convention);

            return listener;
        }

        class StubExecutionSummaryListener :
            Handler<CaseCompleted>,
            Handler<ClassCompleted>,
            Handler<AssemblyCompleted>
        {
            public TimeSpan ExpectedDuration { get; private set; }
            public ExecutionSummary SummaryOfClass { get; private set; }
            public ExecutionSummary SummaryOfAssembly { get; private set; }

            public void Handle(CaseCompleted message) => ExpectedDuration += message.Duration;
            public void Handle(ClassCompleted message) => SummaryOfClass = message.Summary;
            public void Handle(AssemblyCompleted message) => SummaryOfAssembly = message.Summary;
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