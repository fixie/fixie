using System;
using System.IO;
using System.Reflection;
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
            var convention = new SelfTestConvention();
            convention.CaseExecution.Skip(x => x.Method.Has<SkipAttribute>(), x => x.Method.GetCustomAttribute<SkipAttribute>().Reason);
            var assemblyResult = runner.RunType(GetType().Assembly, convention, typeof(PassFailTestClass));
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
            using (var xmlReader = XmlReader.Create(Path.Combine("Reports", "NUnitXmlReport.xsd")))
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
                var fileLocation = PathToThisFile();
                return XDocument.Parse(File.ReadAllText(Path.Combine("Reports", "NUnitXmlReport.xml")))
                                .ToString(SaveOptions.DisableFormatting)
                                .Replace("[assemblyLocation]", assemblyLocation)
                                .Replace("[fileLocation]", fileLocation);
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

            [Skip("reason")]
            public void SkipA()
            {
                throw new ShouldBeUnreachableException();
            }
        }

        [AttributeUsage(AttributeTargets.Method)]
        class SkipAttribute : Attribute
        {
            public SkipAttribute(string reason)
            {
                Reason = reason;
            }

            public string Reason { get; private set; }
        }
    }
}