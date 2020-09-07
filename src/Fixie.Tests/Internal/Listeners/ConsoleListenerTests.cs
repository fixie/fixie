namespace Fixie.Tests.Internal.Listeners
{
    using System.Linq;
    using Fixie.Internal.Listeners;
    using Assertions;

    public class ConsoleListenerTests : MessagingTests
    {
        public void ShouldReportResults()
        {
            var listener = new ConsoleListener();

            Run(listener, out var console);

            console
                .CleanStackTraceLineNumbers()
                .CleanDuration()
                .ShouldBe(
                    "Console.Out: Fail",
                    "Console.Error: Fail",
                    "Test '" + TestClass + ".Fail' failed:",
                    "",
                    "'Fail' failed!",
                    "",
                    "Fixie.Tests.FailureException",
                    At("Fail()"),
                    "",

                    "Console.Out: FailByAssertion",
                    "Console.Error: FailByAssertion",
                    "Test '" + TestClass + ".FailByAssertion' failed:",
                    "",
                    "Expected: 2",
                    "Actual:   1",
                    "",
                    "Fixie.Tests.Assertions.AssertException",
                    At("FailByAssertion()"),
                    "",

                    "Console.Out: Pass",
                    "Console.Error: Pass",

                    "Test '" + TestClass + ".SkipWithReason' skipped:",
                    "⚠ Skipped with reason.",
                    "",
                    "Test '" + TestClass + ".SkipWithoutReason' skipped",
                    "",

                    "Test '" + GenericTestClass + ".ShouldBeString<System.Int32>(123)' failed:",
                    "",
                    "Expected: System.String",
                    "Actual:   System.Int32",
                    "",
                    "Fixie.Tests.Assertions.AssertException",
                    At<SampleGenericTestClass>("ShouldBeString[T](T genericArgument)"),
                    "",

                    "2 passed, 3 failed, 2 skipped, took 1.23 seconds");
        }

        class ZeroPassed : SelfTestDiscovery
        {
            public ZeroPassed()
            {
                TestMethodConditions.Add(x => !x.Name.StartsWith("Pass") && x.ReflectedType == TestClassType);
            }
        }

        public void ShouldNotReportPassCountsWhenZeroTestsHavePassed()
        {
            var listener = new ConsoleListener();
            var discovery = new ZeroPassed();

            Run(listener, discovery, out var console);

            console
                .CleanDuration()
                .Last()
                .ShouldBe("2 failed, 2 skipped, took 1.23 seconds");
        }

        class ZeroFailed : SelfTestDiscovery
        {
            public ZeroFailed()
            {
                TestMethodConditions.Add(x => !x.Name.StartsWith("Fail") && x.ReflectedType == TestClassType);
            }
        }

        public void ShouldNotReportFailCountsWhenZeroTestsHaveFailed()
        {
            var listener = new ConsoleListener();
            var discovery = new ZeroFailed();

            Run(listener, discovery, out var console);

            console
                .CleanDuration()
                .Last()
                .ShouldBe("1 passed, 2 skipped, took 1.23 seconds");
        }

        class ZeroSkipped : SelfTestDiscovery
        {
            public ZeroSkipped()
            {
                TestMethodConditions.Add(x => !x.Name.StartsWith("Skip") && x.ReflectedType == TestClassType);
            }
        }

        public void ShouldNotReportSkipCountsWhenZeroTestsHaveBeenSkipped()
        {
            var listener = new ConsoleListener();
            var discovery = new ZeroSkipped();

            Run(listener, discovery, out var console);

            console
                .CleanDuration()
                .Last()
                .ShouldBe("1 passed, 2 failed, took 1.23 seconds");
        }

        class NoTestsFound : Discovery
        {
            public NoTestsFound()
            {
                TestMethodConditions.Add(x => false);
            }
        }

        public void ShouldProvideDiagnosticDescriptionWhenNoTestsWereExecuted()
        {
            var listener = new ConsoleListener();
            var discovery = new NoTestsFound();

            Run(listener, discovery, out var console);

            console
                .Last()
                .ShouldBe("No tests found.");
        }
    }
}