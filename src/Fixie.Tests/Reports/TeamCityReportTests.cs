namespace Fixie.Tests.Reports
{
    using System;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Assertions;
    using Fixie.Reports;

    public class TeamCityReportTests : MessagingTests
    {
        public async Task ShouldReportResultsToTheConsoleInTeamCityFormat()
        {
            var eol = Environment.NewLine == "\r\n" ? "|r|n" : "|n";

            var report = new TeamCityReport();

            var output = await RunAsync(report);

            output.Console
                .CleanStackTraceLineNumbers()
                .Select(x => Regex.Replace(x, @"duration='\d+'", "duration='#'"))
                .ShouldBe(
                    "##teamcity[testSuiteStarted name='Fixie.Tests']",

                    "Console.Out: Fail",
                    "Console.Error: Fail",
                    $"##teamcity[testStarted name='{TestClass}.Fail']",
                    $"##teamcity[testStdOut name='{TestClass}.Fail' out='Console.Out: Fail{eol}Console.Error: Fail{eol}']",
                    $"##teamcity[testFailed name='{TestClass}.Fail' message='|'Fail|' failed!' details='Fixie.Tests.FailureException{eol}{At("Fail()")}']",
                    $"##teamcity[testFinished name='{TestClass}.Fail' duration='#']",

                    "Console.Out: FailByAssertion",
                    "Console.Error: FailByAssertion",
                    $"##teamcity[testStarted name='{TestClass}.FailByAssertion']",
                    $"##teamcity[testStdOut name='{TestClass}.FailByAssertion' out='Console.Out: FailByAssertion{eol}Console.Error: FailByAssertion{eol}']",
                    $"##teamcity[testFailed name='{TestClass}.FailByAssertion' message='Expected: 2{eol}Actual:   1' details='Fixie.Tests.Assertions.AssertException{eol}{At("FailByAssertion()")}']",
                    $"##teamcity[testFinished name='{TestClass}.FailByAssertion' duration='#']",

                    "Console.Out: Pass",
                    "Console.Error: Pass",
                    $"##teamcity[testStarted name='{TestClass}.Pass']",
                    $"##teamcity[testStdOut name='{TestClass}.Pass' out='Console.Out: Pass{eol}Console.Error: Pass{eol}']",
                    $"##teamcity[testFinished name='{TestClass}.Pass' duration='#']",
                    
                    $"##teamcity[testStarted name='{TestClass}.SkipWithReason']",
                    $"##teamcity[testIgnored name='{TestClass}.SkipWithReason' message='|0x26a0 Skipped with reason.']",
                    $"##teamcity[testFinished name='{TestClass}.SkipWithReason' duration='#']",
                    
                    $"##teamcity[testStarted name='{TestClass}.SkipWithoutReason']",
                    $"##teamcity[testIgnored name='{TestClass}.SkipWithoutReason' message='']",
                    $"##teamcity[testFinished name='{TestClass}.SkipWithoutReason' duration='#']",

                    $"##teamcity[testStarted name='{GenericTestClass}.ShouldBeString<System.String>(\"abc\")']",
                    $"##teamcity[testFinished name='{GenericTestClass}.ShouldBeString<System.String>(\"abc\")' duration='#']",

                    $"##teamcity[testStarted name='{GenericTestClass}.ShouldBeString<System.Int32>(123)']",
                    $"##teamcity[testFailed name='{GenericTestClass}.ShouldBeString<System.Int32>(123)' message='Expected: System.String{eol}Actual:   System.Int32' details='Fixie.Tests.Assertions.AssertException{eol}{At<SampleGenericTestClass>("ShouldBeString|[T|](T genericArgument)")}']",
                    $"##teamcity[testFinished name='{GenericTestClass}.ShouldBeString<System.Int32>(123)' duration='#']",

                    "##teamcity[testSuiteFinished name='Fixie.Tests']");
        }
    }
}