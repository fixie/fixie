namespace Fixie.Tests.Runner.Reports
{
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Schema;
    using Assertions;
    using Fixie.Execution;
    using Fixie.Runner.Reports;

    public class NUnitXmlTests : MessagingTests
    {
        public void ShouldProduceValidXmlDocument()
        {
            XDocument actual = null;

            var listener = new ReportListener<NUnitXml>(assembly => actual = new NUnitXml().Transform(assembly));

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

            XsdValidate(actual);
            CleanBrittleValues(actual.ToString(SaveOptions.DisableFormatting)).ShouldEqual(ExpectedReport);
        }

        static void XsdValidate(XDocument doc)
        {
            var schemaSet = new XmlSchemaSet();
            using (var xmlReader = XmlReader.Create(Path.Combine("Runner", Path.Combine("Reports", "NUnitXmlReport.xsd"))))
            {
                schemaSet.Add(null, xmlReader);
            }

            doc.Validate(schemaSet, null);
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
            cleaned = cleaned.CleanStackTraceLineNumbers();

            //Avoid brittle assertion introduced by environment attributes.
            cleaned = Regex.Replace(cleaned, @"clr-version=""[^""]*""", @"clr-version=""[clr-version]""");
            cleaned = Regex.Replace(cleaned, @"os-version=""[^""]*""", @"os-version=""[os-version]""");
            cleaned = Regex.Replace(cleaned, @"platform=""[^""]*""", @"platform=""[platform]""");
            cleaned = Regex.Replace(cleaned, @"cwd=""[^""]*""", @"cwd=""[cwd]""");
            cleaned = Regex.Replace(cleaned, @"machine-name=""[^""]*""", @"machine-name=""[machine-name]""");
            cleaned = Regex.Replace(cleaned, @"user=""[^""]*""", @"user=""[user]""");
            cleaned = Regex.Replace(cleaned, @"user-domain=""[^""]*""", @"user-domain=""[user-domain]""");

            //Avoid brittle assertion introduced by culture attributes.
            cleaned = Regex.Replace(cleaned, @"current-culture=""[^""]*""", @"current-culture=""[current-culture]""");
            cleaned = Regex.Replace(cleaned, @"current-uiculture=""[^""]*""", @"current-uiculture=""[current-uiculture]""");

            return cleaned;
        }

        string ExpectedReport
        {
            get
            {
                var assemblyLocation = GetType().Assembly.Location;
                var fileLocation = TestClassPath();
                return XDocument.Parse(File.ReadAllText(Path.Combine("Runner", Path.Combine("Reports", "NUnitXmlReport.xml"))))
                                .ToString(SaveOptions.DisableFormatting)
                                .Replace("[assemblyLocation]", assemblyLocation)
                                .Replace("[fileLocation]", fileLocation)
                                .Replace("[testClass]", TestClass)
                                .Replace("[testClassForStackTrace]", TestClass.Replace("+", "."));
            }
        }
    }
}