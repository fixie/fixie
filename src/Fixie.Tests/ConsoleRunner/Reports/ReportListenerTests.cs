namespace Fixie.Tests.ConsoleRunner.Reports
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Text.RegularExpressions;
    using System.Threading;
    using Fixie.ConsoleRunner.Reports;
    using Fixie.Execution;
    using Fixie.Internal;
    using Should;
    using static Utility;

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

                var testClass = FullName<PassFailTestClass>();

                var report = listener.Report;

                report.Passed.ShouldEqual(3);
                report.Failed.ShouldEqual(2);
                report.Skipped.ShouldEqual(2);
                report.Total.ShouldEqual(7);

                report.Assemblies.Count.ShouldEqual(1);

                var assemblyReport = report.Assemblies.Single();
                assemblyReport.Location.ShouldEqual(typeof(ReportListenerTests).Assembly.Location);
                assemblyReport.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);
                assemblyReport.Passed.ShouldEqual(3);
                assemblyReport.Failed.ShouldEqual(2);
                assemblyReport.Skipped.ShouldEqual(2);
                assemblyReport.Total.ShouldEqual(7);

                var classReport = assemblyReport.Classes.Single();
                classReport.Name.ShouldEqual(testClass);
                classReport.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);
                classReport.Passed.ShouldEqual(3);
                classReport.Failed.ShouldEqual(2);
                classReport.Skipped.ShouldEqual(2);

                var cases = classReport.Cases;

                cases.Count.ShouldEqual(7);

                var skipWithReason = (CaseSkipped)cases[0];
                var skipWithoutReason = (CaseSkipped)cases[1];
                var failA = (CaseFailed)cases[2];
                var failB = (CaseFailed)cases[3];
                var passA = cases[4];
                var passB = cases[5];
                var passC = cases[6];

                skipWithReason.MethodGroup.FullName.ShouldEqual(testClass + ".SkipWithReason");
                skipWithReason.Name.ShouldEqual(testClass + ".SkipWithReason");
                skipWithReason.Status.ShouldEqual(CaseStatus.Skipped);
                skipWithReason.Duration.ShouldEqual(TimeSpan.Zero);
                skipWithReason.Output.ShouldEqual(null);
                skipWithReason.Reason.ShouldEqual("Skipped with reason.");

                skipWithoutReason.MethodGroup.FullName.ShouldEqual(testClass + ".SkipWithoutReason");
                skipWithoutReason.Name.ShouldEqual(testClass + ".SkipWithoutReason");
                skipWithoutReason.Status.ShouldEqual(CaseStatus.Skipped);
                skipWithoutReason.Duration.ShouldEqual(TimeSpan.Zero);
                skipWithoutReason.Output.ShouldEqual(null);
                skipWithoutReason.Reason.ShouldEqual(null);

                failA.MethodGroup.FullName.ShouldEqual(testClass + ".FailA");
                failA.Name.ShouldEqual(testClass + ".FailA");
                failA.Status.ShouldEqual(CaseStatus.Failed);
                failA.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);
                failA.Output.Lines().ShouldEqual("Console.Out: FailA", "Console.Error: FailA");
                failA.Exception.Type.ShouldEqual("Fixie.Tests.FailureException");
                failA.Exception.Message.ShouldEqual("'FailA' failed!");
                failA.Exception.FailedAssertion.ShouldEqual(false);
                CleanBrittleValues(failA.Exception.StackTrace)
                    .ShouldEqual(At<PassFailTestClass>("FailA()"));

                failB.MethodGroup.FullName.ShouldEqual(testClass + ".FailB");
                failB.Name.ShouldEqual(testClass + ".FailB");
                failB.Status.ShouldEqual(CaseStatus.Failed);
                failB.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);
                failB.Output.Lines().ShouldEqual("Console.Out: FailB", "Console.Error: FailB");
                failB.Exception.Type.ShouldEqual("Fixie.Tests.FailureException");
                failB.Exception.Message.ShouldEqual("'FailB' failed!");
                failB.Exception.FailedAssertion.ShouldEqual(false);
                CleanBrittleValues(failB.Exception.StackTrace)
                    .ShouldEqual(At<PassFailTestClass>("FailB()"));

                passA.MethodGroup.FullName.ShouldEqual(testClass + ".PassA");
                passA.Name.ShouldEqual(testClass + ".PassA");
                passA.Status.ShouldEqual(CaseStatus.Passed);
                passA.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);
                passA.Output.Lines().ShouldEqual("Console.Out: PassA", "Console.Error: PassA");

                passB.MethodGroup.FullName.ShouldEqual(testClass + ".PassB");
                passB.Name.ShouldEqual(testClass + ".PassB");
                passB.Status.ShouldEqual(CaseStatus.Passed);
                passB.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);
                passB.Output.Lines().ShouldEqual("Console.Out: PassB", "Console.Error: PassB");

                passC.MethodGroup.FullName.ShouldEqual(testClass + ".PassC");
                passC.Name.ShouldEqual(testClass + ".PassC");
                passC.Status.ShouldEqual(CaseStatus.Passed);
                passC.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);
                passC.Output.Lines().ShouldEqual("Console.Out: PassC", "Console.Error: PassC");

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