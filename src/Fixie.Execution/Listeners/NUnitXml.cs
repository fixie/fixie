namespace Fixie.Execution.Listeners
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;
    using Execution;

    public class NUnitXml : XmlFormat
    {
        public string Name => "NUnit";

        public XDocument Transform(AssemblyReport assemblyReport)
        {
            var now = DateTime.UtcNow;

            return new XDocument(
                new XElement("test-results",
                    new XAttribute("date", now.ToString("yyyy-MM-dd")),
                    new XAttribute("time", now.ToString("HH:mm:ss")),
                    new XAttribute("name", "Results"),
                    new XAttribute("total", assemblyReport.Total),
                    new XAttribute("failures", assemblyReport.Failed),
                    new XAttribute("not-run", assemblyReport.Skipped),

                    //Fixie has fewer test states than NUnit, so these counts are always zero.
                    new XAttribute("errors", 0), //Already accounted for by "failures" above.
                    new XAttribute("inconclusive", 0), //No such status.
                    new XAttribute("ignored", 0), //Already accounted for by "not-run" above.
                    new XAttribute("skipped", 0), //Already accounted for by "not-run" above.
                    new XAttribute("invalid", 0), //Already accounted for by "failures" above.

                    Environment(),
                    CultureInfo(),
                    Assembly(assemblyReport)));
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
                new XAttribute("clr-version", Env.Version),
                new XAttribute("os-version", Env.OSVersion),
                new XAttribute("platform", Env.OSVersionPlatform),
                new XAttribute("cwd", Directory.GetCurrentDirectory()),
                new XAttribute("machine-name", Env.MachineName),
                new XAttribute("user", Env.UserName),
                new XAttribute("user-domain", Env.UserDomainName));
        }

        static XElement Assembly(AssemblyReport assemblyReport)
        {
            return new XElement("test-suite",
                new XAttribute("type", "Assembly"),
                new XAttribute("success", assemblyReport.Failed == 0),
                new XAttribute("name", assemblyReport.Assembly.Location),
                new XAttribute("time", Seconds(assemblyReport.Duration)),
                new XAttribute("executed", true),
                new XAttribute("result", assemblyReport.Failed > 0 ? "Failure" : "Success"),
                new XElement("results", assemblyReport.Classes.Select(Class)));
        }

        static XElement Class(ClassReport classReport)
        {
            return new XElement("test-suite",
                new XAttribute("type", "TestFixture"),
                new XAttribute("name", classReport.TestClass.FullName),
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

            if (message.Status == CaseStatus.Skipped)
            {
                var skip = (CaseSkipped)message;
                if (skip.Reason != null)
                    @case.Add(new XElement("reason", new XElement("message", new XCData(skip.Reason))));
            }

            if (message.Status == CaseStatus.Failed)
            {
                var exception = ((CaseFailed)message).Exception;
                @case.Add(
                    new XElement("failure",
                        new XElement("message", new XCData(exception.Message)),
                        new XElement("stack-trace", new XCData(exception.TypedStackTrace()))));
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
