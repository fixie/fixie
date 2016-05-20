namespace Fixie.Tests.ConsoleRunner
{
    using System.Linq;
    using System.Text.RegularExpressions;
    using Fixie.ConsoleRunner;
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

                console.Lines()
                       .Select(x => x.CleanStackTraceLineNumbers()) //Avoid brittle assertion introduced by stack trace line numbers.
                       .Select(x => Regex.Replace(x, @"duration='\d+'", "duration='#'")) //Avoid brittle assertion introduced by durations.
                       .ShouldEqual(
                           "##teamcity[testSuiteStarted name='Fixie.Tests']",
                           "##teamcity[testIgnored name='" + testClass + ".SkipWithReason' message='Skipped with reason.']",
                           "##teamcity[testIgnored name='" + testClass + ".SkipWithoutReason' message='']",

                           "Console.Out: Fail",
                           "Console.Error: Fail",
                           "Console.Out: FailByAssertion",
                           "Console.Error: FailByAssertion",
                           "Console.Out: Pass",
                           "Console.Error: Pass",

                           "##teamcity[testStarted name='"+testClass+".Fail']",
                           "##teamcity[testStdOut name='" + testClass + ".Fail' out='Console.Out: Fail|r|nConsole.Error: Fail|r|n']",
                           "##teamcity[testFailed name='" + testClass + ".Fail' message='|'Fail|' failed!' details='" + At<SampleTestClass>("Fail()") + "']",
                           "##teamcity[testFinished name='" + testClass + ".Fail' duration='#']",
                           "##teamcity[testStarted name='" + testClass + ".FailByAssertion']",
                           "##teamcity[testStdOut name='" + testClass + ".FailByAssertion' out='Console.Out: FailByAssertion|r|nConsole.Error: FailByAssertion|r|n']",
                           "##teamcity[testFailed name='" + testClass + ".FailByAssertion' message='Assert.Equal() Failure|r|nExpected: 2|r|nActual:   1' details='" + At<SampleTestClass>("FailByAssertion()") + "']",
                           "##teamcity[testFinished name='" + testClass + ".FailByAssertion' duration='#']",
                           "##teamcity[testStarted name='" + testClass + ".Pass']",
                           "##teamcity[testStdOut name='" + testClass + ".Pass' out='Console.Out: Pass|r|nConsole.Error: Pass|r|n']",
                           "##teamcity[testFinished name='" + testClass + ".Pass' duration='#']",
                           "##teamcity[testSuiteFinished name='Fixie.Tests']");
            }
        }
    }
}