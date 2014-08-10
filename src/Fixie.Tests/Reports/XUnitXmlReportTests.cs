using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using Fixie.Execution;
using Fixie.Reports;
using Fixie.Results;
using Should;

namespace Fixie.Tests.Reports
{
    public class XUnitXmlReportTests
    {
        public void ShouldProduceValidXmlDocument()
        {
            var listener = new StubListener();
            var runner = new Runner(listener);

            var executionResult = new ExecutionResult();
            var convention = SelfTestConvention.Build();
            convention.CaseExecution.Skip(x => x.Method.Has<SkipAttribute>(), x => x.Method.GetCustomAttribute<SkipAttribute>().Reason);
            convention.Parameters.Add<FromParametersAttribute>();
            var assemblyResult = runner.RunTypes(GetType().Assembly, convention, typeof(PassFailTestClass));
            executionResult.Add(assemblyResult);

            var report = new XUnitXmlReport();
            var actual = report.Transform(executionResult);

            XsdValidate(actual);
            CleanBrittleValues(actual.ToString(SaveOptions.DisableFormatting)).ShouldEqual(ExpectedReport);
        }

        class FromParametersAttribute : ParameterSource
        {
            public IEnumerable<object[]> GetParameters(MethodInfo method)
            {
                return method.GetCustomAttributes<ParametersAttribute>().Select(x => x.Parameters);
            }
        }

        static void XsdValidate(XDocument doc)
        {
            var schemaSet = new XmlSchemaSet();
            using (var xmlReader = XmlReader.Create(Path.Combine("Reports", "XUnitXmlReport.xsd")))
            {
                schemaSet.Add(null, xmlReader);
            }

            doc.Validate(schemaSet, null);
        }

        static string CleanBrittleValues(string actualRawContent)
        {
            //Avoid brittle assertion introduced by system date.
            var cleaned = Regex.Replace(actualRawContent, @"run-date=""\d\d\d\d-\d\d-\d\d""", @"run-date=""YYYY-MM-DD""");

            //Avoid brittle assertion introduced by system time.
            cleaned = Regex.Replace(cleaned, @"run-time=""\d\d:\d\d:\d\d""", @"run-time=""HH:MM:SS""");

            //Avoid brittle assertion introduced by .NET version.
            cleaned = Regex.Replace(cleaned, @"environment=""\d+-bit \.NET [\.\d]+""", @"environment=""00-bit .NET 1.2.3.4""");

            //Avoid brittle assertion introduced by fixie version.
            cleaned = Regex.Replace(cleaned, @"test-framework=""Fixie \d+\.\d+\.\d+\.\d+""", @"test-framework=""Fixie 1.2.3.4""");

            //Avoid brittle assertion introduced by test duration.
            cleaned = Regex.Replace(cleaned, @"time=""[\d\.]+""", @"time=""1.234""");

            //Avoid brittle assertion introduced by stack trace line numbers.
            cleaned = Regex.Replace(cleaned, @":line \d+", ":line #");

            return cleaned;
        }

        string ExpectedReport
        {
            get
            {
                var assemblyLocation = GetType().Assembly.Location;
                var configLocation = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
                var fileLocation = PathToThisFile();
                return XDocument.Parse(File.ReadAllText(Path.Combine("Reports", "XUnitXmlReport.xml")))
                                .ToString(SaveOptions.DisableFormatting)
                                .Replace("[assemblyLocation]", assemblyLocation)
                                .Replace("[configLocation]", configLocation)
                                .Replace("[fileLocation]", fileLocation);
            }
        }

        static string PathToThisFile([CallerFilePath] string path = null)
        {
            return path;
        }

        class PassFailTestClass
        {
            public void Fail()
            {
                throw new FailureException();
            }

            public void Pass() { }

            [Parameters(false)]
            [Parameters(true)]
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

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
        class ParametersAttribute : Attribute
        {
            public ParametersAttribute(params object[] parameters)
            {
                Parameters = parameters;
            }

            public object[] Parameters { get; private set; }
        }

        [AttributeUsage(AttributeTargets.Method)]
        class SkipAttribute : Attribute
        {
            public SkipAttribute()
            {
            }

            public SkipAttribute(string reason)
            {
                Reason = reason;
            }

            public string Reason { get; private set; }
        }
    }
}