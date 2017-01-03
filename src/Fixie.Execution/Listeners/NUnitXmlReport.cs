namespace Fixie.Execution.Listeners
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Xml.Linq;
    using Execution;

    public class NUnitXmlReport
    {
        public XDocument Transform(ExecutionReport executionReport)
        {
            var now = DateTime.UtcNow;

            return new XDocument(
                new XElement("test-results",
                    new XAttribute("date", now.ToString("yyyy-MM-dd")),
                    new XAttribute("time", now.ToString("HH:mm:ss")),
                    new XAttribute("name", "Results"),
                    new XAttribute("total", executionReport.Total),
                    new XAttribute("failures", executionReport.Failed),
                    new XAttribute("not-run", executionReport.Skipped),

                    //Fixie has fewer test states than NUnit, so these counts are always zero.
                    new XAttribute("errors", 0), //Already accounted for by "failures" above.
                    new XAttribute("inconclusive", 0), //No such status.
                    new XAttribute("ignored", 0), //Already accounted for by "not-run" above.
                    new XAttribute("skipped", 0), //Already accounted for by "not-run" above.
                    new XAttribute("invalid", 0), //Already accounted for by "failures" above.

                    Environment(),
                    CultureInfo(),
                    executionReport.Assemblies.Select(Assembly)));
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
                new XAttribute("nunit-version", "2.6.4"), //The NUnit version whose XSD we are complying with.
                new XAttribute("clr-version", System.Environment.Version.ToString()),
                new XAttribute("os-version", System.Environment.OSVersion.ToString()),
                new XAttribute("platform", System.Environment.OSVersion.Platform.ToString()),
                new XAttribute("cwd", System.Environment.CurrentDirectory),
                new XAttribute("machine-name", System.Environment.MachineName),
                new XAttribute("user", System.Environment.UserName),
                new XAttribute("user-domain", System.Environment.UserDomainName));
        }

        static XElement Assembly(AssemblyReport assemblyReport)
        {
            return new XElement("test-suite",
                new XAttribute("type", "Assembly"),
                new XAttribute("success", assemblyReport.Failed == 0),
                new XAttribute("name", assemblyReport.Location),
                new XAttribute("time", Seconds(assemblyReport.Duration)),
                new XAttribute("executed", true),
                new XAttribute("result", assemblyReport.Failed > 0 ? "Failure" : "Success"),
                new XElement("results", assemblyReport.Classes.Select(Class)));
        }

        static XElement Class(ClassReport classReport)
        {
            return new XElement("test-suite",
                new XAttribute("type", "TestFixture"),
                new XAttribute("name", classReport.Name),
                new XAttribute("success", classReport.Failed == 0),
                new XAttribute("time", Seconds(classReport.Duration)),
                new XAttribute("executed", true),
                new XAttribute("result", classReport.Failed > 0 ? "Failure" : "Success"),
                new XElement("results", classReport.Cases.Select(Case)));
        }

        static XElement Case(CaseCompleted message)
        {
            var @case = new XElement("test-case",
                new XAttribute("name", message.Name),
                new XAttribute("executed", message.Status != CaseStatus.Skipped),
                new XAttribute("success", message.Status != CaseStatus.Failed),
                new XAttribute("result", Result(message.Status)));

            if (message.Status != CaseStatus.Skipped)
                @case.Add(new XAttribute("time", Seconds(message.Duration)));

            if (message.Status == CaseStatus.Skipped && message.SkipReason != null)
                @case.Add(new XElement("reason", new XElement("message", new XCData(message.SkipReason))));

            if (message.Status == CaseStatus.Failed)
            {
                @case.Add(
                    new XElement("failure",
                        new XElement("message", new XCData(message.Exceptions.PrimaryException.Message)),
                        new XElement("stack-trace", new XCData(message.Exceptions.CompoundStackTrace))));
            }

            return @case;
        }

        static string Result(CaseStatus status)
        {
            switch (status)
            {
                case CaseStatus.Passed:
                    return "Success";
                case CaseStatus.Failed:
                    return "Failure";
                case CaseStatus.Skipped:
                    return "Ignored";
                default:
                    throw new ArgumentOutOfRangeException(nameof(status));
            }
        }

        static string Seconds(TimeSpan duration)
        {
            //The XML Schema spec requires decimal values to use a culture-ignorant format.
            return duration.TotalSeconds.ToString("0.000", NumberFormatInfo.InvariantInfo);
        }
    }
}
