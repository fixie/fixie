namespace Fixie.Execution.Listeners
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Xml.Linq;

    public class ReportListener :
        Handler<CaseCompleted>,
        Handler<ClassCompleted>,
        Handler<AssemblyCompleted>
    {
        readonly Action<XDocument> save;

        ExecutionSummary currentClassSummary;
        List<XElement> currentClass;

        ExecutionSummary summary;
        List<XElement> classes;

        public ReportListener(Action<XDocument> save)
        {
            this.save = save;

            currentClassSummary = new ExecutionSummary();
            currentClass = new List<XElement>();

            summary = new ExecutionSummary();
            classes = new List<XElement>();
        }

        public void Handle(CaseCompleted message)
        {
            currentClassSummary.Add(message);
            currentClass.Add(Case(message));
            summary.Add(message);
        }

        public void Handle(ClassCompleted message)
        {
            classes.Add(Class(message, currentClassSummary, currentClass));

            currentClassSummary = new ExecutionSummary();
            currentClass = new List<XElement>();
        }

        public void Handle(AssemblyCompleted message)
        {
            save(Report(message, summary, classes));

            summary = null;
            classes = null;
        }

        static XDocument Report(AssemblyCompleted message, ExecutionSummary summary, List<XElement> classes)
        {
            var now = DateTime.UtcNow;

            return new XDocument(
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
                        classes)));
        }

#if NET452
        static string ConfigFile => AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
        static string Framework => Environment.Version.ToString();
#else
        static string ConfigFile => "N/A";
        static string Framework => "Core";
#endif

        static XElement Class(ClassCompleted message, ExecutionSummary summary, List<XElement> cases)
        {
            return new XElement("class",
                new XAttribute("time", Seconds(summary.Duration)),
                new XAttribute("name", message.Class.FullName),
                new XAttribute("total", summary.Failed + summary.Passed + summary.Skipped),
                new XAttribute("passed", summary.Passed),
                new XAttribute("failed", summary.Failed),
                new XAttribute("skipped", summary.Skipped),
                cases);
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