namespace Fixie.Tests.Execution.Listeners
{
    using System.IO;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Schema;
    using Fixie.Execution.Listeners;
    using Should;
    using static Utility;

    public class NUnitXmlTests
    {
        public void ShouldProduceValidXmlDocument()
        {
            XDocument actual = null;

            var listener = new ReportListener<NUnitXml>(report => actual = new NUnitXml().Transform(report));

            var convention = SelfTestConvention.Build();
            convention.CaseExecution.Skip(x => x.Method.Has<SkipAttribute>(), x => x.Method.GetCustomAttribute<SkipAttribute>().Reason);
            convention.Parameters.Add<InputAttributeParameterSource>();
            typeof(PassFailTestClass).Run(listener, convention);

            XsdValidate(actual);
            CleanBrittleValues(actual.ToString(SaveOptions.DisableFormatting)).ShouldEqual(ExpectedReport);
        }

        static void XsdValidate(XDocument doc)
        {
            var schemaSet = new XmlSchemaSet();
            using (var xmlReader = XmlReader.Create(Path.Combine("Execution", Path.Combine("Listeners", "NUnitXmlReport.xsd"))))
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
            cleaned = Regex.Replace(cleaned, @":line \d+", ":line #");

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
                var fileLocation = PathToThisFile();
                return XDocument.Parse(File.ReadAllText(Path.Combine("Execution", Path.Combine("Listeners", "NUnitXmlReport.xml"))))
                                .ToString(SaveOptions.DisableFormatting)
                                .Replace("[assemblyLocation]", assemblyLocation)
                                .Replace("[fileLocation]", fileLocation);
            }
        }

        class PassFailTestClass
        {
            public void Fail()
            {
                throw new FailureException();
            }

            public void Pass() { }

            [Input(false)]
            [Input(true)]
            public void PassIfTrue(bool pass)
            {
                if (!pass) throw new FailureException();
            }

            [Skip]
            public void SkipWithoutReason()
            {
                throw new ShouldBeUnreachableException();
            }

            [Skip("reason")]
            public void SkipWithReason()
            {
                throw new ShouldBeUnreachableException();
            }
        }
    }
}