using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Fixie.Conventions;
using Fixie.Listeners;

namespace Fixie.Tests.Listeners
{
    public class TeamCityListenerTests
    {
        public void ShouldReportResultsToTheConsoleInTeamCityFormat()
        {
            using (var console = new RedirectedConsole())
            {
                var listener = new TeamCityListener();

                new SelfTestConvention().Execute(listener, typeof(PassFailTestClass));

                var testClass = typeof(PassFailTestClass).FullName;

                console.Lines()
                       .Select(x => Regex.Replace(x, @":line \d+", ":line #")) //Avoid brittle assertion introduced by stack trace line numbers.
                       .Select(x => Regex.Replace(x, @"duration='\d+'", "duration='#'")) //Avoid brittle assertion introduced by durations.
                       .ShouldEqual(
                           "##teamcity[testIgnored name='" + testClass + ".SkipA']",

                           "Console.Out: FailA",
                           "Console.Error: FailA",
                           "Console.Out: FailB",
                           "Console.Error: FailB",
                           "Console.Out: PassA",
                           "Console.Error: PassA",
                           "Console.Out: PassB",
                           "Console.Error: PassB",
                           "Console.Out: PassC",
                           "Console.Error: PassC",

                           "##teamcity[testStarted name='"+testClass+".FailA']",
                           "##teamcity[testStdOut name='" + testClass + ".FailA' out='Console.Out: FailA|r|nConsole.Error: FailA|r|n']",
                           "##teamcity[testFailed name='" + testClass + ".FailA' message='|'FailA|' failed!' details='|'FailA|' failed!|r|n   at Fixie.Tests.Listeners.TeamCityListenerTests.PassFailTestClass.FailA() in " + PathToThisFile() + ":line #']",
                           "##teamcity[testFinished name='" + testClass + ".FailA' duration='#']",
                           "##teamcity[testStarted name='" + testClass + ".FailB']",
                           "##teamcity[testStdOut name='" + testClass + ".FailB' out='Console.Out: FailB|r|nConsole.Error: FailB|r|n']",
                           "##teamcity[testFailed name='" + testClass + ".FailB' message='|'FailB|' failed!' details='|'FailB|' failed!|r|n   at Fixie.Tests.Listeners.TeamCityListenerTests.PassFailTestClass.FailB() in " + PathToThisFile() + ":line #']",
                           "##teamcity[testFinished name='" + testClass + ".FailB' duration='#']",
                           "##teamcity[testStarted name='" + testClass + ".PassA']",
                           "##teamcity[testStdOut name='" + testClass + ".PassA' out='Console.Out: PassA|r|nConsole.Error: PassA|r|n']",
                           "##teamcity[testFinished name='" + testClass + ".PassA' duration='#']",
                           "##teamcity[testStarted name='" + testClass + ".PassB']",
                           "##teamcity[testStdOut name='" + testClass + ".PassB' out='Console.Out: PassB|r|nConsole.Error: PassB|r|n']",
                           "##teamcity[testFinished name='" + testClass + ".PassB' duration='#']",
                           "##teamcity[testStarted name='" + testClass + ".PassC']",
                           "##teamcity[testStdOut name='" + testClass + ".PassC' out='Console.Out: PassC|r|nConsole.Error: PassC|r|n']",
                           "##teamcity[testFinished name='" + testClass + ".PassC' duration='#']");

            }
        }

        static string PathToThisFile([CallerFilePath] string path=null)
        {
            return path;
        }

        class PassFailTestClass
        {
            public void FailA()
            {
                WhereAmI();
                throw new FailureException();
            }

            public void PassA()
            {
                WhereAmI();
            }

            public void FailB()
            {
                WhereAmI();
                throw new FailureException();
            }

            public void PassB() { WhereAmI(); }

            public void PassC() { WhereAmI(); }

            public void SkipA() { throw new ShouldBeUnreachableException(); }

            static void WhereAmI([CallerMemberName] string member = null)
            {
                Console.Out.WriteLine("Console.Out: " + member);
                Console.Error.WriteLine("Console.Error: " + member);
            }
        }
    }
}