namespace Fixie.Tests.ConsoleRunner
{
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading;
    using Fixie.ConsoleRunner;
    using Fixie.Internal;
    using static Utility;

    public class ConsoleListenerTests
    {
        public void ShouldReportResultsToTheConsole()
        {
            var listener = new ConsoleListener();
            var convention = SampleTestClassConvention.Build();
            var testClass = FullName<SampleTestClass>();

            using (var console = new RedirectedConsole())
            {
                typeof(SampleTestClass).Run(listener, convention);

                console.Lines()
                       .Select(x => x.CleanStackTraceLineNumbers())
                       .Select(CleanBrittleValues)
                       .ShouldEqual(
                           "------ Testing Assembly Fixie.Tests.dll ------",
                           "",
                           "Test '" + testClass + ".SkipWithReason' skipped: Skipped with reason.",
                           "Test '" + testClass + ".SkipWithoutReason' skipped",
                           "Console.Out: Fail",
                           "Console.Error: Fail",
                           "Console.Out: FailByAssertion",
                           "Console.Error: FailByAssertion",
                           "Console.Out: Pass",
                           "Console.Error: Pass",

                           "Test '" + testClass + ".Fail' failed: Fixie.Tests.FailureException",
                           "'Fail' failed!",
                           At<SampleTestClass>("Fail()"),
                           "",
                           "Test '" + testClass + ".FailByAssertion' failed:",
                           "Assert.Equal() Failure",
                           "Expected: 2",
                           "Actual:   1",
                           At<SampleTestClass>("FailByAssertion()"),
                           "",
                           "1 passed, 2 failed, 2 skipped, took 1.23 seconds (" + Framework.Version + ").");
            }
        }

        static string CleanBrittleValues(string actualRawContent)
        {
            //Avoid brittle assertion introduced by test duration.
            var decimalSeparator = Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            var cleaned = Regex.Replace(actualRawContent, @"took [\d" + Regex.Escape(decimalSeparator) + @"]+ seconds", @"took 1.23 seconds");

            return cleaned;
        }
    }
}