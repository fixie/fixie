namespace Fixie.Tests.Internal.Listeners
{
    using System;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Xml.Linq;
    using Assertions;
    using Fixie.Internal.Listeners;

    public class ReportListenerTests : MessagingTests
    {
        public void ShouldProduceValidXmlDocument()
        {
            XDocument? actual = null;
            var listener = new ReportListener(report => actual = report);

            Run(listener, out var console);

            console
                .ShouldBe(
                    "Console.Out: Fail",
                    "Console.Error: Fail",
                    "Console.Out: FailByAssertion",
                    "Console.Error: FailByAssertion",
                    "Console.Out: Pass",
                    "Console.Error: Pass");

            if (actual == null)
                throw new Exception("Expected non-null XML report.");

            CleanBrittleValues(actual.ToString(SaveOptions.DisableFormatting))
                .Lines()
                .CleanStackTraceLineNumbers()
                .ShouldBe(ExpectedReport.Lines());
        }

        static string CleanBrittleValues(string actualRawContent)
        {
            //Avoid brittle assertion introduced by system date.
            var cleaned = Regex.Replace(actualRawContent, @"run-date=""\d\d\d\d-\d\d-\d\d""", @"run-date=""YYYY-MM-DD""");

            //Avoid brittle assertion introduced by system time.
            cleaned = Regex.Replace(cleaned, @"run-time=""\d\d:\d\d:\d\d""", @"run-time=""HH:MM:SS""");

            //Avoid brittle assertion introduced by .NET version.
            cleaned = cleaned.Replace($@"environment=""{IntPtr.Size * 8}-bit .NET {Framework}""", @"environment=""00-bit .NET 1.2.3.4""");

            //Avoid brittle assertion introduced by fixie version.
            cleaned = cleaned.Replace($@"test-framework=""{Fixie.Framework.Version}""", @"test-framework=""Fixie 1.2.3.4""");

            //Avoid brittle assertion introduced by test duration.
            cleaned = Regex.Replace(cleaned, @"time=""[\d\.]+""", @"time=""1.234""");

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