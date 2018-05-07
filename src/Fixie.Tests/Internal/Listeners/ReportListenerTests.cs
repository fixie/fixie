namespace Fixie.Tests.Internal.Listeners
{
    using System;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Xml.Linq;
    using Assertions;
    using Fixie.Internal;
    using Fixie.Internal.Listeners;

    public class ReportListenerTests : MessagingTests
    {
        public void ShouldProduceValidXmlDocument()
        {
            XDocument actual = null;
            var listener = new ReportListener(report => actual = report);

            using (var console = new RedirectedConsole())
            {
                Run(listener);

                console.Lines()
                    .ShouldEqual(
                        "Console.Out: Fail",
                        "Console.Error: Fail",
                        "Console.Out: FailByAssertion",
                        "Console.Error: FailByAssertion",
                        "Console.Out: Pass",
                        "Console.Error: Pass");
            }

            CleanBrittleValues(actual.ToString(SaveOptions.DisableFormatting)).ShouldEqual(ExpectedReport);
        }

        static string CleanBrittleValues(string actualRawContent)
        {
            //Avoid brittle assertion introduced by system date.
            var cleaned = Regex.Replace(actualRawContent, @"run-date=""\d\d\d\d-\d\d-\d\d""", @"run-date=""YYYY-MM-DD""");

            //Avoid brittle assertion introduced by system time.
            cleaned = Regex.Replace(cleaned, @"run-time=""\d\d:\d\d:\d\d""", @"run-time=""HH:MM:SS""");

            //Avoid brittle assertion introduced by .NET version.
            cleaned = Regex.Replace(cleaned, $@"environment=""{IntPtr.Size * 8}-bit \.NET {Framework}""", @"environment=""00-bit .NET 1.2.3.4""");

            //Avoid brittle assertion introduced by fixie version.
            cleaned = Regex.Replace(cleaned, $@"test-framework=""{Fixie.Framework.Version}""", @"test-framework=""Fixie 1.2.3.4""");

            //Avoid brittle assertion introduced by test duration.
            cleaned = Regex.Replace(cleaned, @"time=""[\d\.]+""", @"time=""1.234""");

            //Avoid brittle assertion introduced by stack trace line numbers.
            cleaned = cleaned.CleanStackTraceLineNumbers();

            return cleaned;
        }

        string ExpectedReport
        {
            get
            {
                var assemblyLocation = GetType().Assembly.Location;
                var fileLocation = TestClassPath();
                return XDocument.Parse(File.ReadAllText(Path.Combine("Internal", Path.Combine("Listeners", "XUnitXmlReport.xml"))))
                                .ToString(SaveOptions.DisableFormatting)
                                .Replace("[assemblyLocation]", assemblyLocation)
                                .Replace("[fileLocation]", fileLocation)
                                .Replace("[testClass]", TestClass)
                                .Replace("[testClassForStackTrace]", TestClass.Replace("+", "."));
            }
        }

        static string Framework => Environment.Version.ToString();
    }
}