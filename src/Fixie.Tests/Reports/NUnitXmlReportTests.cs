﻿using Fixie.Execution;
using Fixie.Internal;
using Fixie.Reports;
using Should;
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

namespace Fixie.Tests.Reports
{
    public class NUnitXmlReportTests
    {
        public void ShouldProduceValidXmlDocument()
        {
            var listener = new StubListener();
            var runner = new Runner(listener);

            var executionResult = new ExecutionResult();
            var convention = SelfTestConvention.Build();
            convention.CaseExecution.Skip(x => x.Method.Has<SkipAttribute>(), x => x.Method.GetCustomAttribute<SkipAttribute>().Reason);
            convention.Parameters.Add<InputAttributeParameterSource>();
            var assemblyResult = runner.RunTypes(GetType().Assembly, convention, typeof(PassFailTestClass));
            executionResult.Add(assemblyResult);

            var report = new NUnitXmlReport();
            var actual = report.Transform(executionResult);

            XsdValidate(actual);
            CleanBrittleValues(actual.ToString(SaveOptions.DisableFormatting)).ShouldEqual(ExpectedReport);
        }

        class InputAttributeParameterSource : ParameterSource
        {
            public IEnumerable<object[]> GetParameters(MethodInfo method)
            {
                return method.GetCustomAttributes<InputAttribute>().Select(x => x.Parameters);
            }
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

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
        class InputAttribute : Attribute
        {
            public InputAttribute(params object[] parameters)
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