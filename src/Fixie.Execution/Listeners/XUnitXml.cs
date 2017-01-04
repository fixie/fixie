namespace Fixie.Execution.Listeners
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Xml.Linq;
    using Execution;

    public class XUnitXml : XmlFormat
    {
        public XDocument Transform(Report report)
        {
            return new XDocument(
                new XElement("assemblies",
                    Assembly(report)));
        }

        static XElement Assembly(Report report)
        {
            var now = DateTime.UtcNow;

            return new XElement("assembly",
                new XAttribute("name", report.Location),
                new XAttribute("run-date", now.ToString("yyyy-MM-dd")),
                new XAttribute("run-time", now.ToString("HH:mm:ss")),
                new XAttribute("configFile", AppDomain.CurrentDomain.SetupInformation.ConfigurationFile),
                new XAttribute("time", Seconds(report.Duration)),
                new XAttribute("total", report.Total),
                new XAttribute("passed", report.Passed),
                new XAttribute("failed", report.Failed),
                new XAttribute("skipped", report.Skipped),
                new XAttribute("environment", String.Format("{0}-bit .NET {1}", IntPtr.Size * 8, Environment.Version)),
                new XAttribute("test-framework", Framework.Version),
                report.Classes.Select(Class));
        }

        static XElement Class(ClassReport classReport)
        {
            return new XElement("class",
                new XAttribute("time", Seconds(classReport.Duration)),
                new XAttribute("name", classReport.Name),
                new XAttribute("total", classReport.Failed + classReport.Passed + classReport.Skipped),
                new XAttribute("passed", classReport.Passed),
                new XAttribute("failed", classReport.Failed),
                new XAttribute("skipped", classReport.Skipped),
                classReport.Cases.Select(Case));
        }

        static XElement Case(CaseCompleted message)
        {
            var @case = new XElement("test",
                new XAttribute("name", message.Name),
                new XAttribute("type", message.MethodGroup.Class),
                new XAttribute("method", message.MethodGroup.Method),
                new XAttribute("result",
                    message.Status == CaseStatus.Failed
                        ? "Fail"
                        : message.Status == CaseStatus.Passed
                            ? "Pass"
                            : "Skip"));

            if (message.Status != CaseStatus.Skipped)
                @case.Add(new XAttribute("time", Seconds(message.Duration)));

            if (message.Status == CaseStatus.Skipped && message.SkipReason != null)
                @case.Add(new XElement("reason", new XElement("message", new XCData(message.SkipReason))));

            if (message.Status == CaseStatus.Failed)
                @case.Add(
                    new XElement("failure",
                        new XAttribute("exception-type", message.Exceptions.PrimaryException.Type),
                        new XElement("message", new XCData(message.Exceptions.PrimaryException.Message)),
                        new XElement("stack-trace", new XCData(message.Exceptions.CompoundStackTrace))));

            return @case;
        }

        static string Seconds(TimeSpan duration)
        {
            //The XML Schema spec requires decimal values to use a culture-ignorant format.
            return duration.TotalSeconds.ToString("0.000", NumberFormatInfo.InvariantInfo);
        }
    }
}