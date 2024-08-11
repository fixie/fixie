using System.Reflection;
using Fixie.Reports;
using static Fixie.Tests.Utility;

namespace Fixie.Tests.Reports;

public class ConsoleReportTests : MessagingTests
{
    public async Task ShouldReportResults()
    {
        var output = await Run(environment => new ConsoleReport(environment));

        output.Console
            .NormalizeStackTraceLines()
            .CleanDuration()
            .ShouldBe([
                $"Running Fixie.Tests (net{TargetFrameworkVersion})",
                "",

                "Test '" + TestClass + ".Fail' failed:",
                "",
                "'Fail' failed!",
                "",
                "Fixie.Tests.FailureException",
                At("Fail()"),
                "",

                "Test '" + TestClass + ".FailByAssertion' failed:",
                "",
                "x should be 2 but was 1",
                "",
                "Fixie.Tests.Assertions.AssertException",
                At("FailByAssertion()"),
                "",

                "Test '" + TestClass + ".Skip' skipped:",
                "⚠ Skipped with attribute.",
                "",

                "Test '" + GenericTestClass + ".ShouldBeString<System.Int32>(123)' failed:",
                "",
                "genericArgument should be typeof(string) but was typeof(int)",
                "",
                "Fixie.Tests.Assertions.AssertException",
                At<SampleGenericTestClass>("ShouldBeString[T](T genericArgument)"),
                "",

                "3 passed, 3 failed, 1 skipped, took 1.23 seconds"
            ]);
    }

    public async Task ShouldIncludePassingResultsWhenFilteringByPattern()
    {
        var output = await Run(console => new ConsoleReport(console, testPattern: "*"));

        output.Console
            .NormalizeStackTraceLines()
            .CleanDuration()
            .ShouldBe([
                $"Running Fixie.Tests (net{TargetFrameworkVersion})",
                "",

                "Test '" + TestClass + ".Fail' failed:",
                "",
                "'Fail' failed!",
                "",
                "Fixie.Tests.FailureException",
                At("Fail()"),
                "",

                "Test '" + TestClass + ".FailByAssertion' failed:",
                "",
                "x should be 2 but was 1",
                "",
                "Fixie.Tests.Assertions.AssertException",
                At("FailByAssertion()"),
                "",

                "Test '" + TestClass + ".Pass' passed",
                "",

                "Test '" + TestClass + ".Skip' skipped:",
                "⚠ Skipped with attribute.",
                "",

                "Test '" + GenericTestClass + ".ShouldBeString<System.String>(\"A\")' passed",
                "Test '" + GenericTestClass + ".ShouldBeString<System.String>(\"B\")' passed",
                "",

                "Test '" + GenericTestClass + ".ShouldBeString<System.Int32>(123)' failed:",
                "",
                "genericArgument should be typeof(string) but was typeof(int)",
                "",
                "Fixie.Tests.Assertions.AssertException",
                At<SampleGenericTestClass>("ShouldBeString[T](T genericArgument)"),
                "",

                "3 passed, 3 failed, 1 skipped, took 1.23 seconds"
            ]);
    }

    class ZeroPassed : SelfTestDiscovery
    {
        public override IEnumerable<MethodInfo> TestMethods(IEnumerable<MethodInfo> publicMethods)
            => publicMethods.Where(x => !x.Name.StartsWith("Pass") && x.ReflectedType == TestClassType);
    }

    public async Task ShouldNotReportPassCountsWhenZeroTestsHavePassed()
    {
        var discovery = new ZeroPassed();

        var output = await Run(console => new ConsoleReport(console), discovery);

        output.Console
            .CleanDuration()
            .Last()
            .ShouldBe("2 failed, 1 skipped, took 1.23 seconds");
    }

    class ZeroFailed : SelfTestDiscovery
    {
        public override IEnumerable<MethodInfo> TestMethods(IEnumerable<MethodInfo> publicMethods)
            => publicMethods.Where(x => !x.Name.StartsWith("Fail") && x.ReflectedType == TestClassType);
    }

    public async Task ShouldNotReportFailCountsWhenZeroTestsHaveFailed()
    {
        var discovery = new ZeroFailed();

        var output = await Run(console => new ConsoleReport(console), discovery);

        output.Console
            .CleanDuration()
            .Last()
            .ShouldBe("1 passed, 1 skipped, took 1.23 seconds");
    }

    class ZeroSkipped : SelfTestDiscovery
    {
        public override IEnumerable<MethodInfo> TestMethods(IEnumerable<MethodInfo> publicMethods)
            => publicMethods.Where(x => !x.Name.StartsWith("Skip") && x.ReflectedType == TestClassType);
    }

    public async Task ShouldNotReportSkipCountsWhenZeroTestsHaveBeenSkipped()
    {
        var discovery = new ZeroSkipped();

        var output = await Run(console => new ConsoleReport(console), discovery);

        output.Console
            .CleanDuration()
            .Last()
            .ShouldBe("1 passed, 2 failed, took 1.23 seconds");
    }

    class NoTestsFound : SelfTestDiscovery
    {
        public override IEnumerable<MethodInfo> TestMethods(IEnumerable<MethodInfo> publicMethods)
            => publicMethods.Where(x => false);
    }

    public async Task ShouldProvideDiagnosticDescriptionWhenNoTestsWereExecuted()
    {
        var discovery = new NoTestsFound();

        var output = await Run(console => new ConsoleReport(console), discovery);

        output.Console
            .Last()
            .ShouldBe("No tests found.");

        output = await Run(console => new ConsoleReport(console, testPattern: "Ineffective*Pattern"), discovery);

        output.Console
            .Last()
            .ShouldBe("No tests match the specified pattern: Ineffective*Pattern");
    }
}