using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Fixie.Execution;

namespace Fixie.Reports
{
    public class XUnitXmlReport
    {
        public XDocument Transform(Report report)
        {
            return new XDocument(
                new XElement("assemblies",
                    report.Assemblies.Select(Assembly)));
        }

        static XElement Assembly(AssemblyReport assemblyReport)
        {
            var now = DateTime.UtcNow;

            return new XElement("assembly",
                new XAttribute("name", assemblyReport.Name),
                new XAttribute("run-date", now.ToString("yyyy-MM-dd")),
                new XAttribute("run-time", now.ToString("HH:mm:ss")),
                new XAttribute("configFile", AppDomain.CurrentDomain.SetupInformation.ConfigurationFile),
                new XAttribute("time", Seconds(assemblyReport.Duration)),
                new XAttribute("total", assemblyReport.Total),
                new XAttribute("passed", assemblyReport.Passed),
                new XAttribute("failed", assemblyReport.Failed),
                new XAttribute("skipped", assemblyReport.Skipped),
                new XAttribute("environment", $"{IntPtr.Size*8}-bit .NET {Environment.Version}"),
                new XAttribute("test-framework", $"Fixie {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}"),
                assemblyReport.Classes.Select(Class));
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

        static XElement Case(CaseResult caseResult)
        {
            var @case = new XElement("test",
                new XAttribute("name", caseResult.Name),
                new XAttribute("type", caseResult.MethodGroup.Class),
                new XAttribute("method", caseResult.MethodGroup.Method),
                new XAttribute("result",
                    caseResult.Status == CaseStatus.Failed
                        ? "Fail"
                        : caseResult.Status == CaseStatus.Passed
                            ? "Pass"
                            : "Skip"));

            if (caseResult.Status != CaseStatus.Skipped)
                @case.Add(new XAttribute("time", Seconds(caseResult.Duration)));

            if (caseResult.Status == CaseStatus.Skipped && caseResult.Message != null)
                @case.Add(new XElement("reason", new XElement("message", new XCData(caseResult.Message))));

            if (caseResult.Status == CaseStatus.Failed)
                @case.Add(
                    new XElement("failure",
                        new XAttribute("exception-type", caseResult.ExceptionType),
                        new XElement("message", new XCData(caseResult.Message ?? caseResult.ExceptionType)),
                        new XElement("stack-trace", new XCData(caseResult.StackTrace))));

            return @case;
        }

        static string Seconds(TimeSpan duration)
        {
            //The XML Schema spec requires decimal values to use a culture-ignorant format.
            return duration.TotalSeconds.ToString("0.000", NumberFormatInfo.InvariantInfo);
        }
    }
}