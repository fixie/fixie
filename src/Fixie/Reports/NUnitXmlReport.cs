using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Fixie.Execution;

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

                    //Fixie has fewer test states than NUnit, so these counts are always zero.
                    new XAttribute("errors", 0), //Already accounted for by "failures" above.
                    new XAttribute("inconclusive", 0), //No such status.
                    new XAttribute("ignored", 0), //Already accounted for by "not-run" above.
                    new XAttribute("skipped", 0), //Already accounted for by "not-run" above.
                    new XAttribute("invalid", 0), //Already accounted for by "failures" above.

                    Environment(),
                    CultureInfo(),
                    executionResult.AssemblyResults.Select(Assembly)));
        }

        static XElement CultureInfo()
        {
            return new XElement("culture-info",
                new XAttribute("current-culture", System.Globalization.CultureInfo.CurrentCulture.ToString()),
                new XAttribute("current-uiculture", System.Globalization.CultureInfo.CurrentUICulture.ToString()));
        }

        static XElement Environment()
        {
            return new XElement("environment",
                new XAttribute("nunit-version", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()),
                new XAttribute("clr-version", System.Environment.Version.ToString()),
                new XAttribute("os-version", System.Environment.OSVersion.ToString()),
                new XAttribute("platform", System.Environment.OSVersion.Platform.ToString()),
                new XAttribute("cwd", System.Environment.CurrentDirectory),
                new XAttribute("machine-name", System.Environment.MachineName),
                new XAttribute("user", System.Environment.UserName),
                new XAttribute("user-domain", System.Environment.UserDomainName));
        }

        static XElement Assembly(AssemblyResult assemblyResult)
        {
            return new XElement("test-suite",
                new XAttribute("type", "Assembly"),
                new XAttribute("success", assemblyResult.Failed == 0),
                new XAttribute("name", assemblyResult.Name),
                new XAttribute("time", Seconds(assemblyResult.Duration)),
                new XElement("results", assemblyResult.ConventionResults.Select(Convention)));
        }

        static XElement Convention(ConventionResult conventionResult)
        {
            return new XElement("test-suite",
                new XAttribute("type", "TestSuite"),
                new XAttribute("success", conventionResult.Failed == 0),
                new XAttribute("name", conventionResult.Name),
                new XAttribute("time", Seconds(conventionResult.Duration)),
                new XElement("results", conventionResult.ClassResults.Select(Class)));
        }

        static XElement Class(ClassResult classResult)
        {
            return new XElement("test-suite",
                new XAttribute("type", "TestFixture"),
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
    }
}
