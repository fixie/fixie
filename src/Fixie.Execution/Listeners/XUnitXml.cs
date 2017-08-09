namespace Fixie.Execution.Listeners
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;

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
                        report.Classes.Select(x => x.ToXml()))));
        }

#if NET452
        static string ConfigFile => AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
        static string Framework => Environment.Version.ToString();
#else
        static string ConfigFile => "N/A";
        static string Framework => "Core";
#endif

        static string Seconds(TimeSpan duration)
        {
            //The XML Schema spec requires decimal values to use a culture-ignorant format.
            return duration.TotalSeconds.ToString("0.000", NumberFormatInfo.InvariantInfo);
        }
    }
}