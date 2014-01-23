using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using Fixie.Conventions;
using Fixie.Reports;
using Fixie.Results;
using Should;

namespace Fixie.Tests.Reports
{
    public class NUnitXmlReportTests
    {
        public void ShouldProduceValidXmlDocument()
        {
            var listener = new StubListener();
            var runner = new Runner(listener);

            var executionResult = new ExecutionResult();
            var assemblyResult = runner.RunType(GetType().Assembly, new SelfTestConvention(), typeof(PassFailTestClass));
            executionResult.Add(assemblyResult);

            var report = new NUnitXmlReport();
            var actual = report.Transform(executionResult);

            XsdValidate(actual);
            CleanBrittleValues(actual.ToString(SaveOptions.DisableFormatting)).ShouldEqual(ExpectedReport);
        }

        static string CleanBrittleValues(string actualRawContent)
        {
            //Avoid brittle assertion introduced by system date.
            var cleaned = Regex.Replace(actualRawContent, @"date=""\d\d\d\d-\d\d-\d\d""", @"date=""YYYY-MM-DD""");

            //Avoid brittle assertion introduced by system time.
            cleaned = Regex.Replace(cleaned, @"time=""\d\d:\d\d:\d\d""", @"time=""HH:MM:SS""");

            //Avoid brittle assertion introduced by test duration.
            cleaned = Regex.Replace(cleaned, @"time=""[\d\.]+""", @"time=""1.234""");

            //Avoid brittle assertion introduced by stack trace line numbers.
            cleaned = Regex.Replace(cleaned, @":line \d+", ":line #");

            return cleaned;
        }

        static void XsdValidate(XDocument doc)
        {
            var schemaSet = new XmlSchemaSet();
            using (var xmlReader = XmlReader.Create(Path.Combine("Reports", "NUnit2Results.xsd")))
            {
                schemaSet.Add(null, xmlReader);
            }

            doc.Validate(schemaSet, null);
        }

        string ExpectedReport
        {
            get
            {
                var assemblyLocation = GetType().Assembly.Location;

                var expectedReport = @"<test-results date=""YYYY-MM-DD"" time=""HH:MM:SS"" name=""Results"" total=""6"" failures=""2"" not-run=""1"">
  <test-suite success=""false"" name=""" + assemblyLocation + @""" time=""1.234"">
    <results>
      <test-suite success=""false"" name=""Fixie.Conventions.SelfTestConvention"" time=""1.234"">
        <results>
          <test-suite name=""Fixie.Tests.Reports.NUnitXmlReportTests+PassFailTestClass"" success=""false"" time=""1.234"">
            <results>
              <test-case name=""Fixie.Tests.Reports.NUnitXmlReportTests+PassFailTestClass.SkipA"" executed=""false"" success=""true"" />
              <test-case name=""Fixie.Tests.Reports.NUnitXmlReportTests+PassFailTestClass.FailA"" executed=""true"" success=""false"" time=""1.234"">
                <failure>
                  <message><![CDATA['FailA' failed!]]></message>
                  <stack-trace><![CDATA['FailA' failed!
   at Fixie.Tests.Reports.NUnitXmlReportTests.PassFailTestClass.FailA() in " + PathToThisFile() + @":line #]]></stack-trace>
                </failure>
              </test-case>
              <test-case name=""Fixie.Tests.Reports.NUnitXmlReportTests+PassFailTestClass.FailB"" executed=""true"" success=""false"" time=""1.234"">
                <failure>
                  <message><![CDATA['FailB' failed!]]></message>
                  <stack-trace><![CDATA['FailB' failed!
   at Fixie.Tests.Reports.NUnitXmlReportTests.PassFailTestClass.FailB() in " + PathToThisFile() + @":line #]]></stack-trace>
                </failure>
              </test-case>
              <test-case name=""Fixie.Tests.Reports.NUnitXmlReportTests+PassFailTestClass.PassA"" executed=""true"" success=""true"" time=""1.234"" />
              <test-case name=""Fixie.Tests.Reports.NUnitXmlReportTests+PassFailTestClass.PassB"" executed=""true"" success=""true"" time=""1.234"" />
              <test-case name=""Fixie.Tests.Reports.NUnitXmlReportTests+PassFailTestClass.PassC"" executed=""true"" success=""true"" time=""1.234"" />
            </results>
          </test-suite>
        </results>
      </test-suite>
    </results>
  </test-suite>
</test-results>";

                return XDocument.Parse(expectedReport).ToString(SaveOptions.DisableFormatting);
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
                throw new FailureException();
            }

            public void PassA() { }

            public void FailB()
            {
                throw new FailureException();
            }

            public void PassB() { }

            public void PassC() { }

            public void SkipA()
            {
                throw new ShouldBeUnreachableException();
            }
        }
    }
}