namespace Fixie.Tests.Execution.Listeners
{
    using System.Linq;
    using System.Text.RegularExpressions;
    using Fixie.Execution;
    using Fixie.Execution.Listeners;

    public class TeamCityListenerTests : MessagingTests
    {
        public void ShouldReportResultsToTheConsoleInTeamCityFormat()
        {
            var listener = new TeamCityListener();

            using (var console = new RedirectedConsole())
            {
                Run(listener);

                console
                    .Output
                    .CleanStackTraceLineNumbers()
                    .Lines()
                    .Select(x => Regex.Replace(x, @"duration='\d+'", "duration='#'"))
                    .ShouldEqual(
                        "##teamcity[testSuiteStarted name='Fixie.Tests']",

                        "Console.Out: Fail",
                        "Console.Error: Fail",
                        "Console.Out: FailByAssertion",
                        "Console.Error: FailByAssertion",
                        "Console.Out: Pass",
                        "Console.Error: Pass",

                        "##teamcity[testStarted name='" + TestClass + ".Fail']",
                        "##teamcity[testStdOut name='" + TestClass + ".Fail' out='Console.Out: Fail|r|nConsole.Error: Fail|r|n']",
                        "##teamcity[testFailed name='" + TestClass + ".Fail' message='|'Fail|' failed!' details='Fixie.Tests.FailureException|r|n" + At("Fail()") + "']",
                        "##teamcity[testFinished name='" + TestClass + ".Fail' duration='#']",
                        "##teamcity[testStarted name='" + TestClass + ".FailByAssertion']",
                        "##teamcity[testStdOut name='" + TestClass + ".FailByAssertion' out='Console.Out: FailByAssertion|r|nConsole.Error: FailByAssertion|r|n']",
                        "##teamcity[testFailed name='" + TestClass + ".FailByAssertion' message='Assertion Failure|r|nExpected: 2|r|nActual:   1' details='" + At("FailByAssertion()") + "']",
                        "##teamcity[testFinished name='" + TestClass + ".FailByAssertion' duration='#']",
                        "##teamcity[testStarted name='" + TestClass + ".Pass']",
                        "##teamcity[testStdOut name='" + TestClass + ".Pass' out='Console.Out: Pass|r|nConsole.Error: Pass|r|n']",
                        "##teamcity[testFinished name='" + TestClass + ".Pass' duration='#']",
                        "##teamcity[testIgnored name='" + TestClass + ".SkipWithReason' message='Skipped with reason.']",
                        "##teamcity[testIgnored name='" + TestClass + ".SkipWithoutReason' message='']",
                        "##teamcity[testSuiteFinished name='Fixie.Tests']");
            }
        }
    }
}