using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Fixie.ConsoleRunner;

namespace Fixie.Tests.ConsoleRunner
{
    public class ConsoleListenerTests
    {
        public void ShouldReportResultsToTheConsole()
        {
            using (var console = new RedirectedConsole())
            {
                var listener = new ConsoleListener();

                typeof(PassFailTestClass).Run(listener, SelfTestConvention.Build());

                var testClass = typeof(PassFailTestClass).FullName;

                console.Lines()
                       .Select(CleanBrittleValues)
                       .ShouldEqual(
                           "------ Testing Assembly Fixie.Tests.dll ------",
                           "Test '" + testClass + ".SkipA' skipped",
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

                           "Test '" + testClass + ".FailA' failed: Fixie.Tests.FailureException",
                           "'FailA' failed!",
                           "   at Fixie.Tests.ConsoleRunner.ConsoleListenerTests.PassFailTestClass.FailA() in " + PathToThisFile() + ":line #",
                           "Test '" + testClass + ".FailB' failed: Fixie.Tests.FailureException",
                           "'FailB' failed!",
                           "   at Fixie.Tests.ConsoleRunner.ConsoleListenerTests.PassFailTestClass.FailB() in " + PathToThisFile() + ":line #",
                           "3 passed, 2 failed, 1 skipped, took 1.23 seconds (Fixie 1.2.3.4).");
            }
        }

        public void ShouldNotReportSkipCountsWhenZeroTestsHaveBeenSkipped()
        {
            using (var console = new RedirectedConsole())
            {
                var listener = new ConsoleListener();

                var convention = SelfTestConvention.Build();

                convention
                    .Methods
                    .Where(method => method.Name != "SkipA");

                typeof(PassFailTestClass).Run(listener, convention);

                var testClass = typeof(PassFailTestClass).FullName;

                console.Lines()
                       .Select(CleanBrittleValues)
                       .ShouldEqual(
                           "------ Testing Assembly Fixie.Tests.dll ------",
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

                           "Test '" + testClass + ".FailA' failed: Fixie.Tests.FailureException",
                           "'FailA' failed!",
                           "   at Fixie.Tests.ConsoleRunner.ConsoleListenerTests.PassFailTestClass.FailA() in " + PathToThisFile() + ":line #",
                           "Test '" + testClass + ".FailB' failed: Fixie.Tests.FailureException",
                           "'FailB' failed!",
                           "   at Fixie.Tests.ConsoleRunner.ConsoleListenerTests.PassFailTestClass.FailB() in " + PathToThisFile() + ":line #",
                           "3 passed, 2 failed, took 1.23 seconds (Fixie 1.2.3.4).");
            }
        }

        static string CleanBrittleValues(string actualRawContent)
        {
            //Avoid brittle assertion introduced by fixie version.
            var cleaned = Regex.Replace(actualRawContent, @"\(Fixie \d+\.\d+\.\d+\.\d+\)", @"(Fixie 1.2.3.4)");

            //Avoid brittle assertion introduced by test duration.
            cleaned = Regex.Replace(cleaned, @"took [\d\.]+ seconds", @"took 1.23 seconds");

            //Avoid brittle assertion introduced by stack trace line numbers.
            cleaned = Regex.Replace(cleaned, @":line \d+", ":line #");

            return cleaned;
        }

        static string PathToThisFile([CallerFilePath] string path = null)
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