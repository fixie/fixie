namespace Fixie.Tests.Execution.Listeners
{
    using System.Linq;
    using Fixie.Execution;
    using Fixie.Execution.Listeners;
    using Assertions;

    public class ConsoleListenerTests : MessagingTests
    {
        public void ShouldReportResults()
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
                           "Console.Out: Fail",
                           "Console.Error: Fail",
                           "Test '" + TestClass + ".Fail' failed: Fixie.Tests.FailureException",
                           "'Fail' failed!",
                           At("Fail()"),
                           "",

                           "Console.Out: FailByAssertion",
                           "Console.Error: FailByAssertion",
                           "Test '" + TestClass + ".FailByAssertion' failed:",
                           "Assertion Failure",
                           "Expected: 2",
                           "Actual:   1",
                           At("FailByAssertion()"),
                           "",

                           "Console.Out: Pass",
                           "Console.Error: Pass",

                           "Test '" + TestClass + ".SkipWithReason' skipped:",
                           "Skipped with reason.",
                           "",
                           "Test '" + TestClass + ".SkipWithoutReason' skipped",
                           "",
                           "1 passed, 2 failed, 2 skipped, took 1.23 seconds");
            }
        }

        public void ShouldNotReportPassCountsWhenZeroTestsHavePassed()
        {
            void ZeroPassed(Convention convention)
                => convention.Methods.Where(method => !method.Name.StartsWith("Pass"));

            var listener = new ConsoleListener();

            using (var console = new RedirectedConsole())
            {
                Run(listener, ZeroPassed);

                console.Output
                    .Lines()
                    .Last()
                    .CleanDuration()
                    .ShouldEqual("2 failed, 2 skipped, took 1.23 seconds");
            }
        }

        public void ShouldNotReportFailCountsWhenZeroTestsHaveFailed()
        {
            void ZeroFailed(Convention convention)
                => convention.Methods.Where(method => !method.Name.StartsWith("Fail"));

            var listener = new ConsoleListener();

            using (var console = new RedirectedConsole())
            {
                Run(listener, ZeroFailed);

                console.Output
                    .Lines()
                    .Last()
                    .CleanDuration()
                    .ShouldEqual("1 passed, 2 skipped, took 1.23 seconds");
            }
        }

        public void ShouldNotReportSkipCountsWhenZeroTestsHaveBeenSkipped()
        {
            void ZeroSkipped(Convention convention)
                => convention.Methods.Where(method => !method.Name.StartsWith("Skip"));

            var listener = new ConsoleListener();

            using (var console = new RedirectedConsole())
            {
                Run(listener, ZeroSkipped);

                console.Output
                    .Lines()
                    .Last()
                    .CleanDuration()
                    .ShouldEqual("1 passed, 2 failed, took 1.23 seconds");
            }
        }

        public void ShouldProvideDiagnosticDescriptionWhenNoTestsWereExecuted()
        {
            void NoTestsFound(Convention convention)
                => convention.Methods.Where(method => false);

            var listener = new ConsoleListener();

            using (var console = new RedirectedConsole())
            {
                Run(listener, NoTestsFound);

                console.Output
                    .Lines()
                    .Last()
                    .ShouldEqual("No tests found.");
            }
        }
    }
}