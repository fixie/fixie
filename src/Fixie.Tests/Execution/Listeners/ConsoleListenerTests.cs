namespace Fixie.Tests.Execution.Listeners
{
    using Fixie.Execution;
    using Fixie.Execution.Listeners;

    public class ConsoleListenerTests : MessagingTests
    {
        public void ShouldReportResultsToTheConsole()
        {
            var listener = new ConsoleListener();

            using (var console = new RedirectedConsole())
            {
                Run(listener);

                console.Output
                       .CleanStackTraceLineNumbers()
                       .CleanDuration()
                       .Lines()
                       .ShouldEqual(
                           "------ Testing Assembly Fixie.Tests.dll ------",
                           "",
                           "Test '" + TestClass + ".SkipWithReason' skipped: Skipped with reason.",
                           "",
                           "Test '" + TestClass + ".SkipWithoutReason' skipped",
                           "",
                           "Console.Out: Fail",
                           "Console.Error: Fail",
                           "Console.Out: FailByAssertion",
                           "Console.Error: FailByAssertion",
                           "Console.Out: Pass",
                           "Console.Error: Pass",

                           "Test '" + TestClass + ".Fail' failed: Fixie.Tests.FailureException",
                           "'Fail' failed!",
                           At("Fail()"),
                           "",
                           "Test '" + TestClass + ".FailByAssertion' failed:",
                           "Assertion Failure",
                           "Expected: 2",
                           "Actual:   1",
                           At("FailByAssertion()"),
                           "",
                           "1 passed, 2 failed, 2 skipped, took 1.23 seconds (" + Framework.Version + ").");
            }
        }
    }
}