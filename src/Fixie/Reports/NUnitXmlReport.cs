using System;
using System.IO;
using System.Xml.Linq;
using Fixie.Results;

namespace Fixie.Reports
{
    public class NUnitXmlReport
    {
        public XDocument Transform(ExecutionResult executionResult)
        {
            var now = DateTime.UtcNow;

            var root =
                new XElement("test-results",
                    new XAttribute("date", now.ToString("yyyy-MM-dd")),
                    new XAttribute("time", now.ToString("HH:mm:ss")),
                    new XAttribute("name", "Results"),
                    new XAttribute("total", executionResult.Total),
                    new XAttribute("failures", executionResult.Failed),
                    new XAttribute("not-run", executionResult.Skipped));

            foreach (var assemblyResult in executionResult.AssemblyResults)
                root.Add(TestSuiteElement(assemblyResult));

            return new XDocument(root);
        }

        static XElement TestSuiteElement(AssemblyResult assemblyResult)
        {
            var suite = new XElement("test-suite",
                new XAttribute("success", assemblyResult.Failed == 0),
                new XAttribute("name", assemblyResult.Name),
                new XAttribute("time", assemblyResult.Duration.TotalSeconds.ToString("0.000")));

            var results = new XElement("results");

            suite.Add(results);

            foreach (var conventionResult in assemblyResult.ConventionResults)
                results.Add(TestSuiteElement(conventionResult));

            return suite;
        }

        static XElement TestSuiteElement(ConventionResult conventionResult)
        {
            var suite = new XElement("test-suite",
                new XAttribute("success", conventionResult.Failed == 0),
                new XAttribute("name", conventionResult.Name),
                new XAttribute("time", conventionResult.Duration.TotalSeconds.ToString("0.000")));

            var results = new XElement("results");

            suite.Add(results);

            foreach (var classResult in conventionResult.ClassResults)
                results.Add(TestSuiteElement(classResult));

            return suite;
        }

        static XElement TestSuiteElement(ClassResult classResult)
        {
            var suite = new XElement("test-suite",
                new XAttribute("name", classResult.Name),
                new XAttribute("success", classResult.Failed == 0),
                new XAttribute("time", classResult.Duration.TotalSeconds.ToString("0.000")));

            var results = new XElement("results");

            suite.Add(results);

            foreach (var caseResult in classResult.CaseResults)
                results.Add(TestSuiteElement(caseResult));

            return suite;
        }

        static XElement TestSuiteElement(CaseResult caseResult)
        {
            var @case = new XElement("test-case");

            @case.Add(new XAttribute("name", caseResult.Name));

            @case.Add(new XAttribute("executed", caseResult.Status != CaseStatus.Skipped));
            @case.Add(new XAttribute("success", caseResult.Status != CaseStatus.Failed));

            if (caseResult.Status != CaseStatus.Skipped)
                @case.Add(new XAttribute("time", caseResult.Duration.TotalSeconds.ToString("0.000")));

            if (caseResult.Status == CaseStatus.Failed)
            {
                @case.Add(
                    new XElement("failure",
                        new XElement("message", new XCData(caseResult.Message)),
                        new XElement("stack-trace", new XCData(caseResult.StackTrace))));
            }

            if (caseResult.Status == CaseStatus.Skipped)
            {
                @case.Add(
                    new XElement("reason",
                        new XElement("message", new XCData(caseResult.SkipReason ?? string.Empty))));
            }

            return @case;
        }
    }
}
