using Fixie.Execution;
using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

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
                    new XAttribute("errors", 0),
                    new XAttribute("inconclusive", 0),
                    new XAttribute("ignored", 0),
                    new XAttribute("skipped", executionResult.Skipped),
                    new XAttribute("invalid", 0),
                    new XElement("environment",
                        new XAttribute("nunit-version", ""),//TODO
                        new XAttribute("clr-version", ""),//TODO
                        new XAttribute("os-version", ""),//TODO
                        new XAttribute("platform", ""),//TODO
                        new XAttribute("cwd", ""),//TODO
                        new XAttribute("machine-name", ""),//TODO
                        new XAttribute("user", ""),//TODO
                        new XAttribute("user-domain", "")),//TODO
                    new XElement("culture-info",
                        new XAttribute("current-culture", ""),//TODO
                        new XAttribute("current-uiculture", "")),//TODO
                    executionResult.AssemblyResults.Select(Assembly)));
        }

        static XElement Assembly(AssemblyResult assemblyResult)
        {
            return new XElement("test-suite",
                new XAttribute("success", assemblyResult.Failed == 0),
                new XAttribute("name", assemblyResult.Name),
                new XAttribute("time", Seconds(assemblyResult.Duration)),
                new XAttribute("executed", assemblyResult.Passed + assemblyResult.Failed),
                new XAttribute("type", "Namespace"),
                new XAttribute("result", assemblyResult.Failed > 0 ? "Failure" : "Succes"),               
                new XElement("results", assemblyResult.ConventionResults.Select(Convention)));
        }

        static XElement Convention(ConventionResult conventionResult)
        {
            return new XElement("test-suite",
                new XAttribute("success", conventionResult.Failed == 0),
                new XAttribute("name", conventionResult.Name),
                new XAttribute("time", Seconds(conventionResult.Duration)),
                new XAttribute("executed", conventionResult.Passed + conventionResult.Failed),
                new XAttribute("type", "Namespace"),
                new XAttribute("result", conventionResult.Failed > 0 ? "Failure" : "Succes"),
                new XElement("results", conventionResult.ClassResults.Select(Class)));
        }

        static XElement Class(ClassResult classResult)
        {
            return new XElement("test-suite",
                new XAttribute("name", classResult.Name),
                new XAttribute("success", classResult.Failed == 0),
                new XAttribute("time", Seconds(classResult.Duration)),
                new XAttribute("executed", classResult.Passed + classResult.Failed),
                new XAttribute("type", "TestFixture"),
                new XAttribute("result", classResult.Failed > 0 ? "Failure" : "Succes"),
                new XElement("results", classResult.CaseResults.Select(Case)));
        }

        static XElement Case(CaseResult caseResult)
        {
            var @case = new XElement("test-case",
                new XAttribute("name", caseResult.Name),
                new XAttribute("result", Result(caseResult.Status)),
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
                        new XElement("message", new XCData(caseResult.Exceptions.PrimaryException.Message)),
                        new XElement("stack-trace", new XCData(caseResult.Exceptions.CompoundStackTrace))));
            }

            return @case;
        }

        static string Seconds(TimeSpan duration)
        {
            //The XML Schema spec requires decimal values to use a culture-ignorant format.
            return duration.TotalSeconds.ToString("0.000", NumberFormatInfo.InvariantInfo);
        }

        static string Result(CaseStatus status)
        {
            switch(status)
            {
                case CaseStatus.Passed:
                    return "Succes";
                case CaseStatus.Failed:
                    return "Failure";
                case CaseStatus.Skipped:
                    return "Ignored";
                default:
                    return "";
            }
        }
    }
}
