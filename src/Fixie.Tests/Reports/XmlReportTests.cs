namespace Fixie.Tests.Reports;

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Assertions;
using Fixie.Reports;
using static Utility;

public class XmlReportTests : MessagingTests
{
    public async Task ShouldProduceValidXmlDocument()
    {
        var environment = GetTestEnvironment();

        XDocument? actual = null;
        var report = new XmlReport(environment, document => actual = document);

        var output = await Run(report);

        output.Console
            .ShouldBe(
                "Standard Out: Fail",
                "Standard Out: FailByAssertion",
                "Standard Out: Pass");

        if (actual == null)
            throw new Exception("Expected non-null XML report.");

        CleanBrittleValues(actual.ToString())
            .Lines()
            .NormalizeStackTraceLines()
            .ToArray()
            .ShouldBe(ExpectedReport.Lines().ToArray());
    }

    static string CleanBrittleValues(string actualRawContent)
    {
        //Avoid brittle assertion introduced by system date.
        var cleaned = Regex.Replace(actualRawContent, @"run-date=""\d\d\d\d-\d\d-\d\d""", @"run-date=""YYYY-MM-DD""");

        //Avoid brittle assertion introduced by system time.
        cleaned = Regex.Replace(cleaned, @"run-time=""\d\d:\d\d:\d\d""", @"run-time=""HH:MM:SS""");

        //Avoid brittle assertion introduced by .NET version.
        cleaned = cleaned.Replace($@"environment=""{IntPtr.Size * 8}-bit {TargetFramework}""", @"environment=""00-bit .NETCoreApp,Version=vX.Y""");

        //Avoid brittle assertion introduced by fixie version.
        cleaned = cleaned.Replace($@"test-framework=""{Fixie.Internal.Framework.Version}""", @"test-framework=""Fixie 1.2.3.4""");

        //Avoid brittle assertion introduced by test duration.
        cleaned = Regex.Replace(cleaned, @"time=""\d+\.\d\d\d""", @"time=""1.234""");

        return cleaned;
    }

    string ExpectedReport
    {
        get
        {
            var assemblyLocation = GetType().Assembly.Location;
            var fileLocation = NormalizedPath(TestClassPath());
            return XDocument.Parse(File.ReadAllText(Path.Combine("Reports", "XUnitXmlReport.xml")))
                .ToString()
                .Replace("[assemblyLocation]", assemblyLocation)
                .Replace("[fileLocation]", fileLocation)
                .Replace("[testClass]", TestClass)
                .Replace("[genericTestClass]", GenericTestClass)
                .Replace("[testClassForStackTrace]", TestClass.Replace("+", "."))
                .Replace("[genericTestClassForStackTrace]", GenericTestClass.Replace("+", "."));
        }
    }

    static string? TargetFramework
        => typeof(XmlReportTests).Assembly.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName;
}