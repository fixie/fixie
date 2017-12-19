namespace Fixie.Execution.Listeners
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Xml.Linq;

    public class ReportListener :
        Handler<CaseSkipped>,
        Handler<CasePassed>,
        Handler<CaseFailed>,
        Handler<ClassCompleted>,
        Handler<AssemblyCompleted>
    {
        readonly Action<XDocument> save;

        List<XElement> currentClass = new List<XElement>();
        List<XElement> classes = new List<XElement>();

        public ReportListener(Action<XDocument> save)
        {
            this.save = save;
        }

        public void Handle(CaseSkipped message)
        {
            currentClass.Add(
                new XElement("test",
                    new XAttribute("name", message.Name),
                    new XAttribute("type", message.Class.FullName),
                    new XAttribute("method", message.Method.Name),
                    new XAttribute("result", "Skip"),
                    message.Reason != null
                        ? new XElement("reason", new XElement("message", new XCData(message.Reason)))
                        : null));
        }

        public void Handle(CasePassed message)
        {
            currentClass.Add(
                new XElement("test",
                    new XAttribute("name", message.Name),
                    new XAttribute("type", message.Class.FullName),
                    new XAttribute("method", message.Method.Name),
                    new XAttribute("result", "Pass"),
                    new XAttribute("time", Seconds(message.Duration))));
        }

        public void Handle(CaseFailed message)
        {
            XElement Failure(CompoundException exception)
                => new XElement("failure",
                    new XAttribute("exception-type", exception.Type),
                    new XElement("message", new XCData(exception.Message)),
                    new XElement("stack-trace", new XCData(exception.StackTrace)));

            currentClass.Add(
                new XElement("test",
                    new XAttribute("name", message.Name),
                    new XAttribute("type", message.Class.FullName),
                    new XAttribute("method", message.Method.Name),
                    new XAttribute("result", "Fail"),
                    new XAttribute("time", Seconds(message.Duration)),
                    Failure(message.Exception)));
        }

        public void Handle(ClassCompleted message)
        {
            var summary = message.Summary;

            classes.Add(
                new XElement("class",
                    new XAttribute("time", Seconds(summary.Duration)),
                    new XAttribute("name", message.Class.FullName),
                    new XAttribute("total", summary.Total),
                    new XAttribute("passed", summary.Passed),
                    new XAttribute("failed", summary.Failed),
                    new XAttribute("skipped", summary.Skipped),
                    currentClass));

            currentClass = new List<XElement>();
        }

        public void Handle(AssemblyCompleted message)
        {
            var now = DateTime.UtcNow;
            var summary = message.Summary;

            save(new XDocument(
                new XElement("assemblies",
                    new XElement("assembly",
                        new XAttribute("name", message.Assembly.Location),
                        new XAttribute("run-date", now.ToString("yyyy-MM-dd")),
                        new XAttribute("run-time", now.ToString("HH:mm:ss")),
                        new XAttribute("configFile", ConfigFile),
                        new XAttribute("time", Seconds(summary.Duration)),
                        new XAttribute("total", summary.Total),
                        new XAttribute("passed", summary.Passed),
                        new XAttribute("failed", summary.Failed),
                        new XAttribute("skipped", summary.Skipped),
                        new XAttribute("environment", $"{IntPtr.Size * 8}-bit .NET {Framework}"),
                        new XAttribute("test-framework", Fixie.Framework.Version),
                        classes))));

            classes = null;
        }

        static string Framework => Environment.Version.ToString();

#if NET471
        static string ConfigFile => AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
#else
        static string ConfigFile => "N/A";
#endif

        static string Seconds(TimeSpan duration)
        {
            //The XML Schema spec requires decimal values to use a culture-ignorant format.
            return duration.TotalSeconds.ToString("0.000", NumberFormatInfo.InvariantInfo);
        }

        public static void Save(XDocument report, string path)
        {
            var directory = Path.GetDirectoryName(path);

            if (String.IsNullOrEmpty(directory))
                return;

            Directory.CreateDirectory(directory);

            using (var stream = new FileStream(path, FileMode.Create))
            using (var writer = new StreamWriter(stream))
                report.Save(writer, SaveOptions.None);
        }
    }
}