namespace Fixie.Execution.Listeners
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;
    using Execution;

    public static class XUnitXml
    {
        public static void Save(Report report, string path)
        {
            var xDocument = Transform(report);

            var directory = Path.GetDirectoryName(path);

            if (String.IsNullOrEmpty(directory))
                return;

            Directory.CreateDirectory(directory);

            using (var stream = new FileStream(path, FileMode.Create))
            using (var writer = new StreamWriter(stream))
                xDocument.Save(writer, SaveOptions.None);
        }

        public static XDocument Transform(Report report)
        {
            var now = DateTime.UtcNow;

            return new XDocument(
                new XElement("assemblies",
                    new XElement("assembly",
                        new XAttribute("name", report.Assembly.Location),
                        new XAttribute("run-date", now.ToString("yyyy-MM-dd")),
                        new XAttribute("run-time", now.ToString("HH:mm:ss")),
                        new XAttribute("configFile", ConfigFile),
                        new XAttribute("time", Seconds(report.Duration)),
                        new XAttribute("total", report.Total),
                        new XAttribute("passed", report.Passed),
                        new XAttribute("failed", report.Failed),
                        new XAttribute("skipped", report.Skipped),
                        new XAttribute("environment", $"{IntPtr.Size * 8}-bit .NET {Framework}"),
                        new XAttribute("test-framework", Fixie.Framework.Version),
                        report.Classes.Select(Class))));
        }

#if NET452
        static string ConfigFile => AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
        static string Framework => Environment.Version.ToString();
#else
        static string ConfigFile => "N/A";
        static string Framework => "Core";
#endif

        static XElement Class(ClassReport classReport)
        {
            return new XElement("class",
                new XAttribute("time", Seconds(classReport.Duration)),
                new XAttribute("name", classReport.Class.FullName),
                new XAttribute("total", classReport.Failed + classReport.Passed + classReport.Skipped),
                new XAttribute("passed", classReport.Passed),
                new XAttribute("failed", classReport.Failed),
                new XAttribute("skipped", classReport.Skipped),
                classReport.Cases.Select(Case));
        }

        static XElement Case(CaseCompleted message)
        {
            if (message is CaseSkipped skipped)
                return Case(skipped);

            if (message is CasePassed passed)
                return Case(passed);

            if (message is CaseFailed failed)
                return Case(failed);

            return null;
        }

        static XElement Case(CaseSkipped message)
            => new XElement("test",
                new XAttribute("name", message.Name),
                new XAttribute("type", message.Class.FullName),
                new XAttribute("method", message.Method.Name),
                new XAttribute("result", "Skip"),
                message.Reason != null
                    ? new XElement("reason", new XElement("message", new XCData(message.Reason)))
                    : null);

        static XElement Case(CasePassed message)
            => new XElement("test",
                new XAttribute("name", message.Name),
                new XAttribute("type", message.Class.FullName),
                new XAttribute("method", message.Method.Name),
                new XAttribute("result", "Pass"),
                new XAttribute("time", Seconds(message.Duration)));

        static XElement Case(CaseFailed message)
            => new XElement("test",
                new XAttribute("name", message.Name),
                new XAttribute("type", message.Class.FullName),
                new XAttribute("method", message.Method.Name),
                new XAttribute("result", "Fail"),
                new XAttribute("time", Seconds(message.Duration)),
                Failure(message.Exception));

        static XElement Failure(CompoundException exception)
        {
            return new XElement("failure",
                new XAttribute("exception-type", exception.Type),
                new XElement("message", new XCData(exception.Message)),
                new XElement("stack-trace", new XCData(exception.StackTrace)));
        }

        static string Seconds(TimeSpan duration)
        {
            //The XML Schema spec requires decimal values to use a culture-ignorant format.
            return duration.TotalSeconds.ToString("0.000", NumberFormatInfo.InvariantInfo);
        }
    }
}