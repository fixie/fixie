namespace Fixie.Reports
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;
    using Internal;
    using static System.Environment;

    class XmlReport :
        Handler<CaseSkipped>,
        Handler<CasePassed>,
        Handler<CaseFailed>,
        Handler<AssemblyCompleted>
    {
        readonly Action<XDocument> save;

        readonly SortedDictionary<string, ClassResult> report = new SortedDictionary<string, ClassResult>();

        internal static XmlReport? Create()
        {
            var absoluteOrRelativePath = GetEnvironmentVariable("FIXIE:REPORT");

            if (absoluteOrRelativePath != null)
                return new XmlReport(SaveReport(absoluteOrRelativePath));

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

        public XmlReport(Action<XDocument> save)
        {
            this.save = save;
        }

        public void Handle(CaseSkipped message)
            => ForClass(message).Add(message);

        public void Handle(CasePassed message)
            => ForClass(message).Add(message);

        public void Handle(CaseFailed message)
            => ForClass(message).Add(message);

        ClassResult ForClass(CaseCompleted message)
        {
            var testClass = message.Test.Class;

            if (!report.ContainsKey(testClass))
                report.Add(testClass, new ClassResult(testClass));

            return report[testClass];
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
                        new XAttribute("test-framework", Internal.Framework.Version),
                        report.Values.Select(x => x.ToElement())))));

            report.Clear();
        }

        static string Framework => Environment.Version.ToString();

        static string Seconds(TimeSpan duration)
        {
            //The XML Schema spec requires decimal values to use a culture-ignorant format.
            return FormattableString.Invariant($"{duration.TotalSeconds:0.000}");
        }

        public static void Save(XDocument report, string path)
        {
            var directory = Path.GetDirectoryName(path);

            if (string.IsNullOrEmpty(directory))
                return;

            Directory.CreateDirectory(directory);

            using var stream = new FileStream(path, FileMode.Create);
            using var writer = new StreamWriter(stream);
            report.Save(writer, SaveOptions.None);
        }

        class ClassResult
        {
            readonly string name;
            TimeSpan duration = TimeSpan.Zero;
            readonly List<XElement> results = new List<XElement>();
            readonly ExecutionSummary summary = new ExecutionSummary();

            public ClassResult(string name) => this.name = name;
            
            public void Add(CaseSkipped message)
            {
                duration += message.Duration;
                summary.Add(message);

                var test = new XElement("test",
                        new XAttribute("name", message.Name),
                        new XAttribute("type", message.Test.Class),
                        new XAttribute("method", message.Test.Method),
                        new XAttribute("result", "Skip"),
                        new XAttribute("time", Seconds(message.Duration)));

                if (message.Reason != null)
                    test.Add(new XElement("reason", new XCData(message.Reason)));

                results.Add(test);
            }

            public void Add(CasePassed message)
            {
                duration += message.Duration;
                summary.Add(message);
                results.Add(
                    new XElement("test",
                        new XAttribute("name", message.Name),
                        new XAttribute("type", message.Test.Class),
                        new XAttribute("method", message.Test.Method),
                        new XAttribute("result", "Pass"),
                        new XAttribute("time", Seconds(message.Duration))));
            }

            public void Add(CaseFailed message)
            {
                duration += message.Duration;
                summary.Add(message);
                results.Add(
                    new XElement("test",
                        new XAttribute("name", message.Name),
                        new XAttribute("type", message.Test.Class),
                        new XAttribute("method", message.Test.Method),
                        new XAttribute("result", "Fail"),
                        new XAttribute("time", Seconds(message.Duration)),
                        new XElement("failure",
                            new XAttribute("exception-type", message.Exception.GetType().FullName!),
                            new XElement("message", new XCData(message.Exception.Message)),
                            new XElement("stack-trace", new XCData(message.Exception.LiterateStackTrace())))));
            }

            public XElement ToElement()
            {
                return new XElement("collection",
                    new XAttribute("time", Seconds(duration)),
                    new XAttribute("name", name),
                    new XAttribute("total", summary.Total),
                    new XAttribute("passed", summary.Passed),
                    new XAttribute("failed", summary.Failed),
                    new XAttribute("skipped", summary.Skipped),
                    results);
            }
        }
    }
}