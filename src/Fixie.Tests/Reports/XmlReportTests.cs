using System.Text.RegularExpressions;
using System.Xml.Linq;
using Fixie.Reports;
using static Fixie.Tests.Utility;

namespace Fixie.Tests.Reports;

public class XmlReportTests : MessagingTests
{
    public async Task ShouldProduceValidXmlDocument()
    {
        await using var console = new StringWriter();

        var environment = GetTestEnvironment(console);

        XDocument? actual = null;
        var report = new XmlReport(environment, document => actual = document);

        await Run(report);

        console.ToString().ShouldBeEmpty();
        
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

        //Avoid brittle assertion introduced by test duration.
        cleaned = Regex.Replace(cleaned, @"time=""\d+\.\d\d\d""", @"time=""1.234""");

        return cleaned;
    }

    string ExpectedReport
    {
        get
        {
            var assemblyLocation = GetType().Assembly.Location;

            var expected = $@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<assemblies>
<assembly name=""{assemblyLocation}"" run-date=""YYYY-MM-DD"" run-time=""HH:MM:SS"" time=""1.234"" total=""7"" passed=""3"" failed=""3"" skipped=""1"" environment=""64-bit net8.0"" test-framework=""{Fixie.Internal.Framework.Version}"">
  <collection time=""1.234"" name=""{GenericTestClass}"" total=""3"" passed=""2"" failed=""1"" skipped=""0"">
    <test name=""{GenericTestClass}.ShouldBeString&lt;System.String&gt;(&quot;A&quot;)"" type=""{GenericTestClass}"" method=""ShouldBeString"" result=""Pass"" time=""1.234"" />
    <test name=""{GenericTestClass}.ShouldBeString&lt;System.String&gt;(&quot;B&quot;)"" type=""{GenericTestClass}"" method=""ShouldBeString"" result=""Pass"" time=""1.234"" />
    <test name=""{GenericTestClass}.ShouldBeString&lt;System.Int32&gt;(123)"" type=""{GenericTestClass}"" method=""ShouldBeString"" result=""Fail"" time=""1.234"">
      <failure exception-type=""Fixie.Tests.Assertions.AssertException"">
        <message><![CDATA[genericArgument should be System.String but was System.Int32]]></message>
        <stack-trace><![CDATA[{At<SampleGenericTestClass>("ShouldBeString[T](T genericArgument)")}]]></stack-trace>
      </failure>
    </test>
  </collection>
  <collection time=""1.234"" name=""{TestClass}"" total=""4"" passed=""1"" failed=""2"" skipped=""1"">
    <test name=""{TestClass}.Fail"" type=""{TestClass}"" method=""Fail"" result=""Fail"" time=""1.234"">
      <failure exception-type=""Fixie.Tests.FailureException"">
        <message><![CDATA['Fail' failed!]]></message>
        <stack-trace><![CDATA[{At("Fail()")}]]></stack-trace>
      </failure>
    </test>
    <test name=""{TestClass}.FailByAssertion"" type=""{TestClass}"" method=""FailByAssertion"" result=""Fail"" time=""1.234"">
      <failure exception-type=""Fixie.Tests.Assertions.AssertException"">
        <message><![CDATA[x should be 2 but was 1]]></message>
        <stack-trace><![CDATA[{At("FailByAssertion()")}]]></stack-trace>
      </failure>
    </test>
    <test name=""{TestClass}.Pass"" type=""{TestClass}"" method=""Pass"" result=""Pass"" time=""1.234"" />
    <test name=""{TestClass}.Skip"" type=""{TestClass}"" method=""Skip"" result=""Skip"" time=""1.234"">
      <reason><![CDATA[⚠ Skipped with attribute.]]></reason>
    </test>
  </collection>
</assembly>
</assemblies>";

            return XDocument.Parse(expected).ToString();
        }
    }
}