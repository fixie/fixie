namespace Fixie.Tests.Internal.Listeners
{
    using System.Linq;
    using System.Text.RegularExpressions;
    using Fixie.Internal;
    using Fixie.Internal.Listeners;

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
                        "##teamcity[testStarted name='" + TestClass + ".Fail']",
                        "##teamcity[testStdOut name='" + TestClass + ".Fail' out='Console.Out: Fail|r|nConsole.Error: Fail|r|n']",
                        "##teamcity[testFailed name='" + TestClass + ".Fail' message='|'Fail|' failed!' details='Fixie.Tests.FailureException|r|n" + At("Fail()") + "']",
                        "##teamcity[testFinished name='" + TestClass + ".Fail' duration='#']",

                        "Console.Out: FailByAssertion",
                        "Console.Error: FailByAssertion",
                        "##teamcity[testStarted name='" + TestClass + ".FailByAssertion']",
                        "##teamcity[testStdOut name='" + TestClass + ".FailByAssertion' out='Console.Out: FailByAssertion|r|nConsole.Error: FailByAssertion|r|n']",
                        "##teamcity[testFailed name='" + TestClass + ".FailByAssertion' message='Expected: 2|r|nActual:   1' details='Fixie.Assertions.AssertActualExpectedException|r|n" + At("FailByAssertion()") + "']",
                        "##teamcity[testFinished name='" + TestClass + ".FailByAssertion' duration='#']",

                        "Console.Out: Pass",
                        "Console.Error: Pass",
                        "##teamcity[testStarted name='" + TestClass + ".Pass']",
                        "##teamcity[testStdOut name='" + TestClass + ".Pass' out='Console.Out: Pass|r|nConsole.Error: Pass|r|n']",
                        "##teamcity[testFinished name='" + TestClass + ".Pass' duration='#']",

                        "##teamcity[testStarted name='" + TestClass + ".SkipWithReason']",
                        "##teamcity[testIgnored name='" + TestClass + ".SkipWithReason' message='|0x26a0 Skipped with reason.']",
                        "##teamcity[testFinished name='" + TestClass + ".SkipWithReason' duration='#']",


                        "##teamcity[testStarted name='" + TestClass + ".SkipWithoutReason']",
                        "##teamcity[testIgnored name='" + TestClass + ".SkipWithoutReason' message='']",
                        "##teamcity[testFinished name='" + TestClass + ".SkipWithoutReason' duration='#']",

                        "##teamcity[testSuiteFinished name='Fixie.Tests']");
            }
        }
    }
}