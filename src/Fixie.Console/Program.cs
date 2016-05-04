using System;
using System.Linq;
using System.Xml.Linq;
using Fixie.Execution;
using Fixie.Reports;

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

            var summaryListener = new SummaryListener();
            ReportListener reportListener = null;

            if (ShouldProduceReports(options))
                reportListener = new ReportListener();

            foreach (var assemblyPath in commandLineParser.AssemblyPaths)
                Execute(assemblyPath, options, summaryListener, reportListener);

            if (reportListener != null)
                SaveReport(options, reportListener.Report);

            return summaryListener.Summary;
        }

        static bool ShouldProduceReports(Options options)
        {
            return options.Contains(CommandLineOption.NUnitXml) || options.Contains(CommandLineOption.XUnitXml);
        }

        static void SaveReport(Options options, ExecutionReport executionReport)
        {
            if (options.Contains(CommandLineOption.NUnitXml))
            {
                var xDocument = new NUnitXmlReport().Transform(executionReport);

                foreach (var fileName in options[CommandLineOption.NUnitXml])
                    xDocument.Save(fileName, SaveOptions.None);
            }

            if (options.Contains(CommandLineOption.XUnitXml))
            {
                var xDocument = new XUnitXmlReport().Transform(executionReport);

                foreach (var fileName in options[CommandLineOption.XUnitXml])
                    xDocument.Save(fileName, SaveOptions.None);
            }
        }

        static void Execute(string assemblyPath, Options options, SummaryListener summaryListener, ReportListener reportListener)
        {
            using (var environment = new ExecutionEnvironment(assemblyPath))
            {
                if (ShouldUseTeamCityListener(options))
                    environment.Subscribe(new TeamCityListener());
                else
                    environment.Subscribe(new ConsoleListener());

                if (ShouldUseAppVeyorListener())
                    environment.Subscribe(new AppVeyorListener());

                if (reportListener != null)
                    environment.Subscribe(reportListener);

                environment.Subscribe(summaryListener);

                environment.RunAssembly(options);
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