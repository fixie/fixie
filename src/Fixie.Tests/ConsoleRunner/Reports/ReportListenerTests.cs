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
                var convention = SampleTestClassConvention.Build();

                typeof(SampleTestClass).Run(listener, convention);

                var testClass = FullName<SampleTestClass>();

                var report = listener.Report;

                report.Passed.ShouldEqual(1);
                report.Failed.ShouldEqual(2);
                report.Skipped.ShouldEqual(2);
                report.Total.ShouldEqual(5);

                report.Assemblies.Count.ShouldEqual(1);

                var assemblyReport = report.Assemblies.Single();
                assemblyReport.Location.ShouldEqual(typeof(ReportListenerTests).Assembly.Location);
                assemblyReport.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);
                assemblyReport.Passed.ShouldEqual(1);
                assemblyReport.Failed.ShouldEqual(2);
                assemblyReport.Skipped.ShouldEqual(2);
                assemblyReport.Total.ShouldEqual(5);

                var classReport = assemblyReport.Classes.Single();
                classReport.Name.ShouldEqual(testClass);
                classReport.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);
                classReport.Passed.ShouldEqual(1);
                classReport.Failed.ShouldEqual(2);
                classReport.Skipped.ShouldEqual(2);

                var cases = classReport.Cases;

                cases.Count.ShouldEqual(5);

                var skipWithReason = (CaseSkipped)cases[0];
                var skipWithoutReason = (CaseSkipped)cases[1];
                var fail = (CaseFailed)cases[2];
                var failByAssertion = (CaseFailed)cases[3];
                var pass = cases[4];

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

                fail.MethodGroup.FullName.ShouldEqual(testClass + ".Fail");
                fail.Name.ShouldEqual(testClass + ".Fail");
                fail.Status.ShouldEqual(CaseStatus.Failed);
                fail.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);
                fail.Output.Lines().ShouldEqual("Console.Out: Fail", "Console.Error: Fail");
                fail.Exception.Type.ShouldEqual("Fixie.Tests.FailureException");
                fail.Exception.Message.ShouldEqual("'Fail' failed!");
                fail.Exception.FailedAssertion.ShouldEqual(false);
                CleanBrittleValues(fail.Exception.StackTrace)
                    .ShouldEqual(At<SampleTestClass>("Fail()"));

                failByAssertion.MethodGroup.FullName.ShouldEqual(testClass + ".FailByAssertion");
                failByAssertion.Name.ShouldEqual(testClass + ".FailByAssertion");
                failByAssertion.Status.ShouldEqual(CaseStatus.Failed);
                failByAssertion.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);
                failByAssertion.Output.Lines().ShouldEqual("Console.Out: FailByAssertion", "Console.Error: FailByAssertion");
                failByAssertion.Exception.Type.ShouldEqual("Should.Core.Exceptions.EqualException");
                failByAssertion.Exception.Message.Lines().ShouldEqual(
                    "Assert.Equal() Failure",
                    "Expected: 2",
                    "Actual:   1");
                failByAssertion.Exception.FailedAssertion.ShouldEqual(true);
                CleanBrittleValues(failByAssertion.Exception.StackTrace)
                    .ShouldEqual(At<SampleTestClass>("FailByAssertion()"));

                pass.MethodGroup.FullName.ShouldEqual(testClass + ".Pass");
                pass.Name.ShouldEqual(testClass + ".Pass");
                pass.Status.ShouldEqual(CaseStatus.Passed);
                pass.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);
                pass.Output.Lines().ShouldEqual("Console.Out: Pass", "Console.Error: Pass");

                console.Lines().ShouldEqual(
                    "Console.Out: Fail",
                    "Console.Error: Fail",
                    "Console.Out: FailByAssertion",
                    "Console.Error: FailByAssertion",
                    "Console.Out: Pass",
                    "Console.Error: Pass");
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
    }
}