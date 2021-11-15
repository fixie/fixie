namespace Fixie.Reports
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using Internal;
    
    /// <summary>
    /// Writes test results to the specified path, using the xUnit XML format.
    /// </summary>
    public class XmlReport :
        IHandler<TestSkipped>,
        IHandler<TestPassed>,
        IHandler<TestFailed>,
        IHandler<ExecutionCompleted>
    {
        readonly TestEnvironment environment;
        readonly Func<XDocument, Task> save;

        readonly SortedDictionary<string, ClassResult> report = new SortedDictionary<string, ClassResult>();

        public XmlReport(TestEnvironment environment, string absoluteOrRelativePath)
            : this(environment, SaveReport(environment, absoluteOrRelativePath))
        {
        }

        static Func<XDocument, Task> SaveReport(TestEnvironment environment, string absoluteOrRelativePath)
        {
            return report => Save(report, FullPath(environment, absoluteOrRelativePath));
        }

        static string FullPath(TestEnvironment environment, string absoluteOrRelativePath)
        {
            return Path.Combine(environment.RootPath, absoluteOrRelativePath);
        }

        internal XmlReport(TestEnvironment environment, Func<XDocument, Task> save)
        {
            this.environment = environment;
            this.save = save;
        }

        public Task Handle(TestSkipped message)
        {
            ForClass(message).Add(message);
            return Task.CompletedTask;
        }

        public Task Handle(TestPassed message)
        {
            ForClass(message).Add(message);
            return Task.CompletedTask;
        }

        public Task Handle(TestFailed message)
        {
            ForClass(message).Add(message);
            return Task.CompletedTask;
        }

        ClassResult ForClass(TestCompleted message)
        {
            Parse(message.Test, out var type, out _);

            if (!report.ContainsKey(type))
                report.Add(type, new ClassResult(type));

            return report[type];
        }

        public async Task Handle(ExecutionCompleted message)
        {
            var now = DateTime.UtcNow;

            await save(new XDocument(
                new XElement("assemblies",
                    new XElement("assembly",
                        new XAttribute("name", environment.Assembly.Location),
                        new XAttribute("run-date", now.ToString("yyyy-MM-dd")),
                        new XAttribute("run-time", now.ToString("HH:mm:ss")),
                        new XAttribute("time", Seconds(message.Duration)),
                        new XAttribute("total", message.Total),
                        new XAttribute("passed", message.Passed),
                        new XAttribute("failed", message.Failed),
                        new XAttribute("skipped", message.Skipped),
                        new XAttribute("environment", $"{IntPtr.Size * 8}-bit {environment.TargetFramework}"),
                        new XAttribute("test-framework", Internal.Framework.Version),
                        report.Values.Select(x => x.ToElement())))));

            report.Clear();
        }

        static string Seconds(TimeSpan duration)
        {
            //The XML Schema spec requires decimal values to use a culture-ignorant format.
            return FormattableString.Invariant($"{duration.TotalSeconds:0.000}");
        }

        static async Task Save(XDocument report, string path)
        {
            var directory = Path.GetDirectoryName(path);

            if (string.IsNullOrEmpty(directory))
                return;

            Directory.CreateDirectory(directory);

            await using var stream = new FileStream(path, FileMode.Create);
            await using var writer = new StreamWriter(stream);
            await report.SaveAsync(writer, SaveOptions.None, CancellationToken.None);
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
                        new XAttribute("name", message.TestCase),
                        new XAttribute("type", type),
                        new XAttribute("method", method),
                        new XAttribute("result", "Skip"),
                        new XAttribute("time", Seconds(message.Duration)));

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
                        new XAttribute("name", message.TestCase),
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
                        new XAttribute("name", message.TestCase),
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