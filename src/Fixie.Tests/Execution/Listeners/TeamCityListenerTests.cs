namespace Fixie.Tests.Execution.Listeners
{
    using System.Linq;
    using System.Text.RegularExpressions;
    using Fixie.Execution.Listeners;
    using Fixie.Internal;
    using static Utility;

    public class TeamCityListenerTests
    {
        public void ShouldReportResultsToTheConsoleInTeamCityFormat()
        {
            var listener = new TeamCityListener();
            var convention = SampleTestClassConvention.Build();
            var testClass = FullName<SampleTestClass>();

            using (var console = new RedirectedConsole())
            {
                typeof(SampleTestClass).Run(listener, convention);

                console
                    .Output
                    .CleanStackTraceLineNumbers()
                    .Lines()
                    .Select(x => Regex.Replace(x, @"duration='\d+'", "duration='#'"))
                    .ShouldEqual(
                        "##teamcity[testSuiteStarted name='Fixie.Tests.dll']",
                        "##teamcity[testIgnored name='" + testClass + ".SkipWithReason' message='Skipped with reason.']",
                        "##teamcity[testIgnored name='" + testClass + ".SkipWithoutReason' message='']",

                        "Console.Out: Fail",
                        "Console.Error: Fail",
                        "Console.Out: FailByAssertion",
                        "Console.Error: FailByAssertion",
                        "Console.Out: Pass",
                        "Console.Error: Pass",

                        "##teamcity[testStarted name='" + testClass + ".Fail']",
                        "##teamcity[testStdOut name='" + testClass + ".Fail' out='Console.Out: Fail|r|nConsole.Error: Fail|r|n']",
                        "##teamcity[testFailed name='" + testClass + ".Fail' message='|'Fail|' failed!' details='Fixie.Tests.FailureException|r|n|'Fail|' failed!|r|n" + At<SampleTestClass>("Fail()") + "']",
                        "##teamcity[testFinished name='" + testClass + ".Fail' duration='#']",
                        "##teamcity[testStarted name='" + testClass + ".FailByAssertion']",
                        "##teamcity[testStdOut name='" + testClass + ".FailByAssertion' out='Console.Out: FailByAssertion|r|nConsole.Error: FailByAssertion|r|n']",
                        "##teamcity[testFailed name='" + testClass + ".FailByAssertion' message='Assert.Equal() Failure|r|nExpected: 2|r|nActual:   1' details='Assert.Equal() Failure|r|nExpected: 2|r|nActual:   1|r|n" + At<SampleTestClass>("FailByAssertion()") + "']",
                        "##teamcity[testFinished name='" + testClass + ".FailByAssertion' duration='#']",
                        "##teamcity[testStarted name='" + testClass + ".Pass']",
                        "##teamcity[testStdOut name='" + testClass + ".Pass' out='Console.Out: Pass|r|nConsole.Error: Pass|r|n']",
                        "##teamcity[testFinished name='" + testClass + ".Pass' duration='#']",
                        "##teamcity[testSuiteFinished name='Fixie.Tests.dll']");
            }
        }
    }
}