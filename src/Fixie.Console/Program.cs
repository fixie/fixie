using System;
using System.Linq;
using System.Xml.Linq;
using Fixie.Execution;

namespace Fixie.ConsoleRunner
{
    class Program
    {
        const int FatalError = -1;

        [STAThread]
        static int Main(string[] args)
        {
            try
            {
                var commandLineParser = new CommandLineParser(args);

                if (commandLineParser.HasErrors)
                {
                    using (Foreground.Red)
                        foreach (var error in commandLineParser.Errors)
                            Console.WriteLine(error);

                    Console.WriteLine();
                    Console.WriteLine(CommandLineParser.Usage());
                    return FatalError;
                }

                var summary = Execute(commandLineParser);

                return summary.Failed;
            }
            catch (Exception exception)
            {
                using (Foreground.Red)
                    Console.WriteLine($"Fatal Error: {exception}");
                return FatalError;
            }
        }

        static ExecutionSummary Execute(CommandLineParser commandLineParser)
        {
            var options = commandLineParser.Options;

            Report report = null;
            ReportListener reportListener = null;

            if (ShouldProduceReports(options))
            {
                report = new Report();
                reportListener = new ReportListener(report);
            }

            var summary = Execute(commandLineParser.AssemblyPath, options, reportListener);

            if (report != null)
                SaveReport(options, report);

            return summary;
        }

        static bool ShouldProduceReports(Options options)
        {
            return options.Contains(CommandLineOption.NUnitXml) || options.Contains(CommandLineOption.XUnitXml);
        }

        static void SaveReport(Options options, Report report)
        {
            if (options.Contains(CommandLineOption.NUnitXml))
            {
                var xDocument = new NUnitXmlReport().Transform(report);

                foreach (var fileName in options[CommandLineOption.NUnitXml])
                    xDocument.Save(fileName, SaveOptions.None);
            }

            if (options.Contains(CommandLineOption.XUnitXml))
            {
                var xDocument = new XUnitXmlReport().Transform(report);

                foreach (var fileName in options[CommandLineOption.XUnitXml])
                    xDocument.Save(fileName, SaveOptions.None);
            }
        }

        static ExecutionSummary Execute(string assemblyPath, Options options, ReportListener reportListener)
        {
            using (var environment = new ExecutionEnvironment(assemblyPath))
            {
                if (ShouldUseTeamCityListener(options))
                    environment.Subscribe<TeamCityListener>();
                else
                    environment.Subscribe<ConsoleListener>();

                if (ShouldUseAppVeyorListener())
                    environment.Subscribe<AppVeyorListener>();

                if (reportListener != null)
                    environment.Subscribe(reportListener);

                return environment.RunAssembly(options);
            }
        }

        static bool ShouldUseTeamCityListener(Options options)
        {
            var teamCityExplicitlySpecified = options.Contains(CommandLineOption.TeamCity);

            var runningUnderTeamCity = Environment.GetEnvironmentVariable("TEAMCITY_PROJECT_NAME") != null;

            var useTeamCityListener =
                (teamCityExplicitlySpecified && options[CommandLineOption.TeamCity].First() == "on") ||
                (!teamCityExplicitlySpecified && runningUnderTeamCity);

            return useTeamCityListener;
        }

        static bool ShouldUseAppVeyorListener()
        {
            return Environment.GetEnvironmentVariable("APPVEYOR") == "True";
        }
    }
}