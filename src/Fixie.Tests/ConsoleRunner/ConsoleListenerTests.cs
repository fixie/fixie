namespace Fixie.Tests.ConsoleRunner
{
    using Fixie.ConsoleRunner;
    using Fixie.Internal;
    using static Utility;

    public class ConsoleListenerTests
    {
        public void ShouldReportResultsToTheConsole()
        {
            var listener = new ConsoleListener();
            var convention = SampleTestClassConvention.Build();
            var testClass = FullName<SampleTestClass>();

            using (var console = new RedirectedConsole())
            {
                Run<SampleTestClass>(listener, convention);

                console.Output
                       .CleanStackTraceLineNumbers()
                       .CleanDuration()
                       .Lines()
                       .ShouldEqual(
                           "------ Testing Assembly Fixie.Tests.dll ------",
                           "",
                           "Test '" + testClass + ".SkipWithReason' skipped: Skipped with reason.",
                           "Test '" + testClass + ".SkipWithoutReason' skipped",
                           "Console.Out: Fail",
                           "Console.Error: Fail",
                           "Console.Out: FailByAssertion",
                           "Console.Error: FailByAssertion",
                           "Console.Out: Pass",
                           "Console.Error: Pass",

                           "Test '" + testClass + ".Fail' failed: Fixie.Tests.FailureException",
                           "'Fail' failed!",
                           At<SampleTestClass>("Fail()"),
                           "",
                           "Test '" + testClass + ".FailByAssertion' failed:",
                           "Assert.Equal() Failure",
                           "Expected: 2",
                           "Actual:   1",
                           At<SampleTestClass>("FailByAssertion()"),
                           "",
                           "1 passed, 2 failed, 2 skipped, took 1.23 seconds (" + Framework.Version + ").");
            }
        }
    }
}