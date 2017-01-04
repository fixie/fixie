namespace Fixie.Tests.Execution.Listeners
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Text.RegularExpressions;
    using System.Threading;
    using Fixie.Execution;
    using Fixie.Execution.Listeners;
    using Fixie.Internal;
    using Should;

    public class ReportListenerTests
    {
        public void ShouldBuildReport()
        {
            using (var console = new RedirectedConsole())
            {
                var listener = new ReportListener();
                var convention = SelfTestConvention.Build();
                convention.CaseExecution.Skip(x => x.Method.Has<SkipAttribute>(), x => x.Method.GetCustomAttribute<SkipAttribute>().Reason);

                typeof(PassFailTestClass).Run(listener, convention);

                var testClass = typeof(PassFailTestClass).FullName;

                var report = listener.Report;
                report.Location.ShouldEqual(typeof(ReportListenerTests).Assembly.Location);
                report.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);
                report.Passed.ShouldEqual(3);
                report.Failed.ShouldEqual(2);
                report.Skipped.ShouldEqual(2);
                report.Total.ShouldEqual(7);

                var classReport = report.Classes.Single();
                classReport.Name.ShouldEqual(testClass);
                classReport.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);
                classReport.Passed.ShouldEqual(3);
                classReport.Failed.ShouldEqual(2);
                classReport.Skipped.ShouldEqual(2);

                var cases = classReport.Cases;

                cases.Count.ShouldEqual(7);

                cases[0].MethodGroup.FullName.ShouldEqual(testClass + ".SkipWithReason");
                cases[0].Name.ShouldEqual(testClass + ".SkipWithReason");
                cases[0].Status.ShouldEqual(CaseStatus.Skipped);
                cases[0].Duration.ShouldEqual(TimeSpan.Zero);
                cases[0].Output.ShouldEqual(null);
                cases[0].Exceptions.ShouldEqual(null);
                cases[0].SkipReason.ShouldEqual("Skipped with reason.");

                cases[1].MethodGroup.FullName.ShouldEqual(testClass + ".SkipWithoutReason");
                cases[1].Name.ShouldEqual(testClass + ".SkipWithoutReason");
                cases[1].Status.ShouldEqual(CaseStatus.Skipped);
                cases[1].Duration.ShouldEqual(TimeSpan.Zero);
                cases[1].Output.ShouldEqual(null);
                cases[1].Exceptions.ShouldEqual(null);
                cases[1].SkipReason.ShouldEqual(null);

                cases[2].MethodGroup.FullName.ShouldEqual(testClass + ".FailA");
                cases[2].Name.ShouldEqual(testClass + ".FailA");
                cases[2].Status.ShouldEqual(CaseStatus.Failed);
                cases[2].Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);
                cases[2].Output.Lines().ShouldEqual("Console.Out: FailA", "Console.Error: FailA");
                cases[2].Exceptions.PrimaryException.Type.ShouldEqual("Fixie.Tests.FailureException");
                cases[2].Exceptions.PrimaryException.DisplayName.ShouldEqual("Fixie.Tests.FailureException");
                cases[2].Exceptions.PrimaryException.Message.ShouldEqual("'FailA' failed!");
                CleanBrittleValues(cases[2].Exceptions.PrimaryException.StackTrace)
                    .ShouldEqual("   at Fixie.Tests.Execution.Listeners.ReportListenerTests.PassFailTestClass.FailA() in " + PathToThisFile() + ":line #");
                cases[2].Exceptions.PrimaryException.InnerException.ShouldBeNull();
                cases[2].Exceptions.SecondaryExceptions.ShouldBeEmpty();
                CleanBrittleValues(cases[2].Exceptions.CompoundStackTrace).Lines().ShouldEqual(
                    "'FailA' failed!",
                    "   at Fixie.Tests.Execution.Listeners.ReportListenerTests.PassFailTestClass.FailA() in " + PathToThisFile() + ":line #");
                cases[2].SkipReason.ShouldEqual(null);

                cases[3].MethodGroup.FullName.ShouldEqual(testClass + ".FailB");
                cases[3].Name.ShouldEqual(testClass + ".FailB");
                cases[3].Status.ShouldEqual(CaseStatus.Failed);
                cases[3].Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);
                cases[3].Output.Lines().ShouldEqual("Console.Out: FailB", "Console.Error: FailB");
                cases[3].Exceptions.PrimaryException.Type.ShouldEqual("Fixie.Tests.FailureException");
                cases[3].Exceptions.PrimaryException.DisplayName.ShouldEqual("Fixie.Tests.FailureException");
                cases[3].Exceptions.PrimaryException.Message.ShouldEqual("'FailB' failed!");
                CleanBrittleValues(cases[3].Exceptions.PrimaryException.StackTrace)
                    .ShouldEqual("   at Fixie.Tests.Execution.Listeners.ReportListenerTests.PassFailTestClass.FailB() in " + PathToThisFile() + ":line #");
                cases[3].Exceptions.PrimaryException.InnerException.ShouldBeNull();
                cases[3].Exceptions.SecondaryExceptions.ShouldBeEmpty();
                CleanBrittleValues(cases[3].Exceptions.CompoundStackTrace).Lines().ShouldEqual(
                    "'FailB' failed!",
                    "   at Fixie.Tests.Execution.Listeners.ReportListenerTests.PassFailTestClass.FailB() in " + PathToThisFile() + ":line #");
                cases[3].SkipReason.ShouldEqual(null);

                cases[4].MethodGroup.FullName.ShouldEqual(testClass + ".PassA");
                cases[4].Name.ShouldEqual(testClass + ".PassA");
                cases[4].Status.ShouldEqual(CaseStatus.Passed);
                cases[4].Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);
                cases[4].Output.Lines().ShouldEqual("Console.Out: PassA", "Console.Error: PassA");
                cases[4].Exceptions.ShouldEqual(null);
                cases[4].SkipReason.ShouldEqual(null);

                cases[5].MethodGroup.FullName.ShouldEqual(testClass + ".PassB");
                cases[5].Name.ShouldEqual(testClass + ".PassB");
                cases[5].Status.ShouldEqual(CaseStatus.Passed);
                cases[5].Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);
                cases[5].Output.Lines().ShouldEqual("Console.Out: PassB", "Console.Error: PassB");
                cases[5].Exceptions.ShouldEqual(null);
                cases[5].SkipReason.ShouldEqual(null);

                cases[6].MethodGroup.FullName.ShouldEqual(testClass + ".PassC");
                cases[6].Name.ShouldEqual(testClass + ".PassC");
                cases[6].Status.ShouldEqual(CaseStatus.Passed);
                cases[6].Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);
                cases[6].Output.Lines().ShouldEqual("Console.Out: PassC", "Console.Error: PassC");
                cases[6].Exceptions.ShouldEqual(null);
                cases[6].SkipReason.ShouldEqual(null);

                console.Lines().ShouldEqual(
                    "Console.Out: FailA",
                    "Console.Error: FailA",
                    "Console.Out: FailB",
                    "Console.Error: FailB",
                    "Console.Out: PassA",
                    "Console.Error: PassA",
                    "Console.Out: PassB",
                    "Console.Error: PassB",
                    "Console.Out: PassC",
                    "Console.Error: PassC");
            }
        }

        static string CleanBrittleValues(string actualRawContent)
        {
            //Avoid brittle assertion introduced by test duration.
            var decimalSeparator = Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            var cleaned = Regex.Replace(actualRawContent, @"took [\d" + Regex.Escape(decimalSeparator) + @"]+ seconds", @"took 1.23 seconds");

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

            public void PassA() { WhereAmI(); }

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