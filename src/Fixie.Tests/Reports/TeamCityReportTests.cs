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

            var output = await RunAsync(console => new TeamCityReport(console));

            output.Console
                .NormalizeStackTraceLines()
                .Select(x => Regex.Replace(x, @"duration='\d+'", "duration='#'"))
                .ShouldBe(
                    "##teamcity[testSuiteStarted name='Fixie.Tests']",

                    "Standard Out: Fail",
                    $"##teamcity[testStarted name='{TestClass}.Fail']",
                    $"##teamcity[testStdOut name='{TestClass}.Fail' out='Standard Out: Fail{eol}']",
                    $"##teamcity[testFailed name='{TestClass}.Fail' message='|'Fail|' failed!' details='Fixie.Tests.FailureException{eol}{At("Fail()")}']",
                    $"##teamcity[testFinished name='{TestClass}.Fail' duration='#']",

                    "Standard Out: FailByAssertion",
                    $"##teamcity[testStarted name='{TestClass}.FailByAssertion']",
                    $"##teamcity[testStdOut name='{TestClass}.FailByAssertion' out='Standard Out: FailByAssertion{eol}']",
                    $"##teamcity[testFailed name='{TestClass}.FailByAssertion' message='Expected: 2{eol}Actual:   1' details='Fixie.Tests.Assertions.AssertException{eol}{At("FailByAssertion()")}']",
                    $"##teamcity[testFinished name='{TestClass}.FailByAssertion' duration='#']",

                    "Standard Out: Pass",
                    $"##teamcity[testStarted name='{TestClass}.Pass']",
                    $"##teamcity[testStdOut name='{TestClass}.Pass' out='Standard Out: Pass{eol}']",
                    $"##teamcity[testFinished name='{TestClass}.Pass' duration='#']",
                    
                    $"##teamcity[testStarted name='{TestClass}.Skip']",
                    $"##teamcity[testIgnored name='{TestClass}.Skip' message='|0x26a0 Skipped with attribute.']",
                    $"##teamcity[testFinished name='{TestClass}.Skip' duration='#']",
                    
                    $"##teamcity[testStarted name='{GenericTestClass}.ShouldBeString<System.String>(\"A\")']",
                    $"##teamcity[testFinished name='{GenericTestClass}.ShouldBeString<System.String>(\"A\")' duration='#']",
                    
                    $"##teamcity[testStarted name='{GenericTestClass}.ShouldBeString<System.String>(\"B\")']",
                    $"##teamcity[testFinished name='{GenericTestClass}.ShouldBeString<System.String>(\"B\")' duration='#']",

                    $"##teamcity[testStarted name='{GenericTestClass}.ShouldBeString<System.Int32>(123)']",
                    $"##teamcity[testFailed name='{GenericTestClass}.ShouldBeString<System.Int32>(123)' message='Expected: System.String{eol}Actual:   System.Int32' details='Fixie.Tests.Assertions.AssertException{eol}{At<SampleGenericTestClass>("ShouldBeString|[T|](T genericArgument)")}']",
                    $"##teamcity[testFinished name='{GenericTestClass}.ShouldBeString<System.Int32>(123)' duration='#']",

                    "##teamcity[testSuiteFinished name='Fixie.Tests']");
        }
    }
}