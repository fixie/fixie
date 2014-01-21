using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Fixie.Conventions;
using Fixie.Listeners;
using Should;

namespace Fixie.Tests.Listeners
{
    public class ConsoleListenerTests
    {
        public void ShouldReportResultsToTheConsole()
        {
            using (var console = new RedirectedConsole())
            {
                var listener = new ConsoleListener();

                new SelfTestConvention().Execute(listener, typeof(PassFailTestClass));

                var testClass = typeof(PassFailTestClass).FullName;

                console.Lines()
                       .Select(x => Regex.Replace(x, @":line \d+", ":line #")) //Avoid brittle assertion introduced by stack trace line numbers.
                       .ShouldEqual(
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
                           "   at Fixie.Tests.Listeners.ConsoleListenerTests.PassFailTestClass.FailA() in " + PathToThisFile() + ":line #",
                           "Test '" + testClass + ".FailB' failed: Fixie.Tests.FailureException",
                           "'FailB' failed!",
                           "   at Fixie.Tests.Listeners.ConsoleListenerTests.PassFailTestClass.FailB() in " + PathToThisFile() + ":line #");
            }
        }

        public void ShouldReportPassFailSkipCounts()
        {
            using (var console = new RedirectedConsole())
            {
                var listener = new ConsoleListener();
                var assembly = typeof(ConsoleListener).Assembly;
                var version = assembly.GetName().Version;

                var assemblyResult = new AssemblyResult(assembly.Location);
                var conventionResult = new ConventionResult("Fake Convention");
                var classResult = new ClassResult("Fake Class");
                assemblyResult.Add(conventionResult);
                conventionResult.Add(classResult);
                classResult.Add(new CaseResult("A", CaseStatus.Passed, TimeSpan.Zero));
                classResult.Add(new CaseResult("B", CaseStatus.Failed, TimeSpan.Zero));
                classResult.Add(new CaseResult("C", CaseStatus.Failed, TimeSpan.Zero));
                classResult.Add(new CaseResult("D", CaseStatus.Skipped, TimeSpan.Zero));
                classResult.Add(new CaseResult("E", CaseStatus.Skipped, TimeSpan.Zero));
                classResult.Add(new CaseResult("F", CaseStatus.Skipped, TimeSpan.Zero));

                listener.AssemblyCompleted(assembly, assemblyResult);

                console.Lines().ShouldEqual("1 passed, 2 failed, 3 skipped (Fixie " + version + ").");
            }
        }

        public void ShouldNotReportSkipCountsWhenZeroTestsHaveBeenSkipped()
        {
            using (var console = new RedirectedConsole())
            {
                var listener = new ConsoleListener();
                var assembly = typeof(ConsoleListener).Assembly;
                var version = assembly.GetName().Version;

                var assemblyResult = new AssemblyResult(assembly.Location);
                var conventionResult = new ConventionResult("Fake Convention");
                var classResult = new ClassResult("Fake Class");
                assemblyResult.Add(conventionResult);
                conventionResult.Add(classResult);
                classResult.Add(new CaseResult("A", CaseStatus.Passed, TimeSpan.Zero));
                classResult.Add(new CaseResult("B", CaseStatus.Failed, TimeSpan.Zero));
                classResult.Add(new CaseResult("C", CaseStatus.Failed, TimeSpan.Zero));

                listener.AssemblyCompleted(assembly, assemblyResult);

                console.Lines().ShouldEqual("1 passed, 2 failed (Fixie " + version + ").");
            }
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