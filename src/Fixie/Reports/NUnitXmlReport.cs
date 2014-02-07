using System;
using System.Linq;
using System.Xml.Linq;
using Fixie.Results;

namespace Fixie.Reports
{
    public class NUnitXmlReport
    {
        public XDocument Transform(ExecutionResult executionResult)
        {
            var now = DateTime.UtcNow;

            return new XDocument(
                new XElement("test-results",
                    new XAttribute("date", now.ToString("yyyy-MM-dd")),
                    new XAttribute("time", now.ToString("HH:mm:ss")),
                    new XAttribute("name", "Results"),
                    new XAttribute("total", executionResult.Total),
                    new XAttribute("failures", executionResult.Failed),
                    new XAttribute("not-run", executionResult.Skipped),
                    executionResult.AssemblyResults.Select(Assembly)));
        }

        static XElement Assembly(AssemblyResult assemblyResult)
        {
            return new XElement("test-suite",
                new XAttribute("success", assemblyResult.Failed == 0),
                new XAttribute("name", assemblyResult.Name),
                new XAttribute("time", Seconds(assemblyResult.Duration)),
                new XElement("results", assemblyResult.ConventionResults.Select(Convention)));
        }

        static XElement Convention(ConventionResult conventionResult)
        {
            return new XElement("test-suite",
                new XAttribute("success", conventionResult.Failed == 0),
                new XAttribute("name", conventionResult.Name),
                new XAttribute("time", Seconds(conventionResult.Duration)),
                new XElement("results", conventionResult.ClassResults.Select(Class)));
        }

        static XElement Class(ClassResult classResult)
        {
            return new XElement("test-suite",
                new XAttribute("name", classResult.Name),
                new XAttribute("success", classResult.Failed == 0),
                new XAttribute("time", Seconds(classResult.Duration)),
                new XElement("results", classResult.CaseResults.Select(Case)));
        }

        static XElement Case(CaseResult caseResult)
        {
            var @case = new XElement("test-case",
                new XAttribute("name", caseResult.Name),
                new XAttribute("executed", caseResult.Status != CaseStatus.Skipped),
                new XAttribute("success", caseResult.Status != CaseStatus.Failed));

            if (caseResult.Status != CaseStatus.Skipped)
                @case.Add(new XAttribute("time", Seconds(caseResult.Duration)));

            if (caseResult.Status == CaseStatus.Skipped && caseResult.SkipReason != null)
                @case.Add(new XElement("reason", new XElement("message", new XCData(caseResult.SkipReason))));

            if (caseResult.Status == CaseStatus.Failed)
            {
                @case.Add(
                    new XElement("failure",
                        new XElement("message", new XCData(caseResult.Message)),
                        new XElement("stack-trace", new XCData(caseResult.StackTrace))));
            }
            
            return @case;
        }

        static string Seconds(TimeSpan duration)
        {
            return duration.TotalSeconds.ToString("0.000");
        }
    }
}
