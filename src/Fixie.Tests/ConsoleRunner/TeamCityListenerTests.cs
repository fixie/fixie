﻿using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Fixie.ConsoleRunner;
using Fixie.Internal;

namespace Fixie.Tests.ConsoleRunner
{
    public class TeamCityListenerTests
    {
        public void ShouldReportResultsToTheConsoleInTeamCityFormat()
        {
            using (var console = new RedirectedConsole())
            {
                var listener = new TeamCityListener();
                var convention = SelfTestConvention.Build();
                convention.CaseExecution.Skip(x => x.Method.Has<SkipAttribute>(), x => x.Method.GetCustomAttribute<SkipAttribute>().Reason);

                typeof(PassFailTestClass).Run(listener, convention);

                var testClass = typeof(PassFailTestClass).FullName;

                console.Lines()
                       .Select(x => Regex.Replace(x, @":line \d+", ":line #")) //Avoid brittle assertion introduced by stack trace line numbers.
                       .Select(x => Regex.Replace(x, @"duration='\d+'", "duration='#'")) //Avoid brittle assertion introduced by durations.
                       .ShouldEqual(
                           "##teamcity[testSuiteStarted name='Fixie.Tests']",
                           "##teamcity[testIgnored name='" + testClass + ".SkipWithReason' message='Skipped with reason.']",
                           "##teamcity[testIgnored name='" + testClass + ".SkipWithoutReason' message='']",

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
                           "##teamcity[testFailed name='" + testClass + ".FailA' message='|'FailA|' failed!' details='   at Fixie.Tests.ConsoleRunner.TeamCityListenerTests.PassFailTestClass.FailA() in " + PathToThisFile() + ":line #']",
                           "##teamcity[testFinished name='" + testClass + ".FailA' duration='#']",
                           "##teamcity[testStarted name='" + testClass + ".FailB']",
                           "##teamcity[testStdOut name='" + testClass + ".FailB' out='Console.Out: FailB|r|nConsole.Error: FailB|r|n']",
                           "##teamcity[testFailed name='" + testClass + ".FailB' message='|'FailB|' failed!' details='   at Fixie.Tests.ConsoleRunner.TeamCityListenerTests.PassFailTestClass.FailB() in " + PathToThisFile() + ":line #']",
                           "##teamcity[testFinished name='" + testClass + ".FailB' duration='#']",
                           "##teamcity[testStarted name='" + testClass + ".PassA']",
                           "##teamcity[testStdOut name='" + testClass + ".PassA' out='Console.Out: PassA|r|nConsole.Error: PassA|r|n']",
                           "##teamcity[testFinished name='" + testClass + ".PassA' duration='#']",
                           "##teamcity[testStarted name='" + testClass + ".PassB']",
                           "##teamcity[testStdOut name='" + testClass + ".PassB' out='Console.Out: PassB|r|nConsole.Error: PassB|r|n']",
                           "##teamcity[testFinished name='" + testClass + ".PassB' duration='#']",
                           "##teamcity[testStarted name='" + testClass + ".PassC']",
                           "##teamcity[testStdOut name='" + testClass + ".PassC' out='Console.Out: PassC|r|nConsole.Error: PassC|r|n']",
                           "##teamcity[testFinished name='" + testClass + ".PassC' duration='#']",
                           "##teamcity[testSuiteFinished name='Fixie.Tests']");
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

            [Skip]
            public void SkipWithoutReason() { throw new ShouldBeUnreachableException(); }

            [Skip("Skipped with reason.")]
            public void SkipWithReason() { throw new ShouldBeUnreachableException(); }

            static void WhereAmI([CallerMemberName] string member = null)
            {
                Console.Out.WriteLine("Console.Out: " + member);
                Console.Error.WriteLine("Console.Error: " + member);
            }
        }
    }
}