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
        Handler<TestSkipped>,
        Handler<TestPassed>,
        Handler<TestFailed>,
        Handler<AssemblyCompleted>
    {
        readonly Action<XDocument> save;

        readonly SortedDictionary<string, ClassResult> report = new SortedDictionary<string, ClassResult>();

        internal static XmlReport? Create(TestContext context)
        {
            var absoluteOrRelativePath = GetEnvironmentVariable("FIXIE:REPORT");

            if (absoluteOrRelativePath != null)
                return new XmlReport(SaveReport(context, absoluteOrRelativePath));

            return null;
        }

        static Action<XDocument> SaveReport(TestContext context, string absoluteOrRelativePath)
        {
            return report => Save(report, FullPath(context, absoluteOrRelativePath));
        }

        static string FullPath(TestContext context, string absoluteOrRelativePath)
        {
            return Path.Combine(context.RootDirectory, absoluteOrRelativePath);
        }

        public XmlReport(Action<XDocument> save)
        {
            this.save = save;
        }

        public void Handle(TestSkipped message)
            => ForClass(message).Add(message);

        public void Handle(TestPassed message)
            => ForClass(message).Add(message);

        public void Handle(TestFailed message)
            => ForClass(message).Add(message);

        ClassResult ForClass(TestCompleted message)
        {
            Parse(message.Test, out var type, out _);

            if (!report.ContainsKey(type))
                report.Add(type, new ClassResult(type));

            return report[type];
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
            
            public void Add(TestSkipped message)
            {
                duration += message.Duration;
                summary.Add(message);

                Parse(message.Test, out var type, out var method);

                var test = new XElement("test",
                        new XAttribute("name", message.Name),
                        new XAttribute("type", type),
                        new XAttribute("method", method),
                        new XAttribute("result", "Skip"),
                        new XAttribute("time", Seconds(message.Duration)));

                if (message.Reason != null)
                    test.Add(new XElement("reason", new XCData(message.Reason)));

                results.Add(test);
            }

            public void Add(TestPassed message)
            {
                duration += message.Duration;
                summary.Add(message);

                Parse(message.Test, out var type, out var method);

                results.Add(
                    new XElement("test",
                        new XAttribute("name", message.Name),
                        new XAttribute("type", type),
                        new XAttribute("method", method),
                        new XAttribute("result", "Pass"),
                        new XAttribute("time", Seconds(message.Duration))));
            }

            public void Add(TestFailed message)
            {
                duration += message.Duration;
                summary.Add(message);

                Parse(message.Test, out var type, out var method);

                results.Add(
                    new XElement("test",
                        new XAttribute("name", message.Name),
                        new XAttribute("type", type),
                        new XAttribute("method", method),
                        new XAttribute("result", "Fail"),
                        new XAttribute("time", Seconds(message.Duration)),
                        new XElement("failure",
                            new XAttribute("exception-type", message.Reason.GetType().FullName!),
                            new XElement("message", new XCData(message.Reason.Message)),
                            new XElement("stack-trace", new XCData(message.Reason.LiterateStackTrace())))));
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

        static void Parse(string fullName, out string className, out string methodName)
        {
            var indexOfMemberSeparator = fullName.LastIndexOf(".");
            className = fullName.Substring(0, indexOfMemberSeparator);
            methodName = fullName.Substring(indexOfMemberSeparator + 1);
        }
    }
}