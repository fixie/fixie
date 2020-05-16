namespace Fixie.Internal.Listeners
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Xml.Linq;

    class ReportListener :
        Handler<CaseSkipped>,
        Handler<CasePassed>,
        Handler<CaseFailed>,
        Handler<ClassCompleted>,
        Handler<AssemblyCompleted>
    {
        readonly Action<XDocument> save;

        readonly List<XElement> currentClass = new List<XElement>();
        readonly List<XElement> classes = new List<XElement>();

        internal static ReportListener? Create(Options options)
        {
            if (options.Report != null)
                return new ReportListener(SaveReport(options.Report));

            return null;
        }

        static Action<XDocument> SaveReport(string absoluteOrRelativePath)
        {
            return report => Save(report, FullPath(absoluteOrRelativePath));
        }

        static string FullPath(string absoluteOrRelativePath)
        {
            return Path.Combine(Directory.GetCurrentDirectory(), absoluteOrRelativePath);
        }

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
                    new XAttribute("time", Seconds(message.Duration)),
                    message.Reason != null
                        ? new XElement("reason", new XCData(message.Reason))
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
            currentClass.Add(
                new XElement("test",
                    new XAttribute("name", message.Name),
                    new XAttribute("type", message.Class.FullName),
                    new XAttribute("method", message.Method.Name),
                    new XAttribute("result", "Fail"),
                    new XAttribute("time", Seconds(message.Duration)),
                    new XElement("failure",
                        new XAttribute("exception-type", message.Exception.GetType().FullName),
                        new XElement("message", new XCData(message.Exception.Message)),
                        new XElement("stack-trace", new XCData(message.Exception.LiterateStackTrace())))));
        }

        public void Handle(ClassCompleted message)
        {
            classes.Add(
                new XElement("collection",
                    new XAttribute("time", Seconds(message.Duration)),
                    new XAttribute("name", message.Class.FullName),
                    new XAttribute("total", message.Total),
                    new XAttribute("passed", message.Passed),
                    new XAttribute("failed", message.Failed),
                    new XAttribute("skipped", message.Skipped),
                    currentClass));

            currentClass.Clear();
        }

        public void Handle(AssemblyCompleted message)
        {
            var now = DateTime.UtcNow;

            save(new XDocument(
                new XElement("assemblies",
                    new XElement("assembly",
                        new XAttribute("name", message.Assembly.Location),
                        new XAttribute("run-date", now.ToString("yyyy-MM-dd")),
                        new XAttribute("run-time", now.ToString("HH:mm:ss")),
                        new XAttribute("time", Seconds(message.Duration)),
                        new XAttribute("total", message.Total),
                        new XAttribute("passed", message.Passed),
                        new XAttribute("failed", message.Failed),
                        new XAttribute("skipped", message.Skipped),
                        new XAttribute("environment", $"{IntPtr.Size * 8}-bit .NET {Framework}"),
                        new XAttribute("test-framework", Fixie.Framework.Version),
                        classes))));

            classes.Clear();
        }

        static string Framework => Environment.Version.ToString();

        static string Seconds(TimeSpan duration)
        {
            //The XML Schema spec requires decimal values to use a culture-ignorant format.
            return duration.TotalSeconds.ToString("0.000", NumberFormatInfo.InvariantInfo);
        }

        public static void Save(XDocument report, string path)
        {
            var directory = Path.GetDirectoryName(path);

            if (string.IsNullOrEmpty(directory))
                return;

            Directory.CreateDirectory(directory);

            using (var stream = new FileStream(path, FileMode.Create))
            using (var writer = new StreamWriter(stream))
                report.Save(writer, SaveOptions.None);
        }
    }
}