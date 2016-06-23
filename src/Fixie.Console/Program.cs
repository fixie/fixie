namespace Fixie.ConsoleRunner
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Execution;
    using Reports;

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

            var listeners = StatelessListeners(options).ToList();

            listeners.Add(summaryListener);

            listeners.AddRange(ReportingListeners(options));

            using (var environment = new ExecutionEnvironment(commandLineParser.AssemblyPath, listeners))
                environment.RunAssembly(options);

            return summaryListener.Summary;
        }

        static IEnumerable<Listener> StatelessListeners(Options options)
        {
            if (ShouldUseTeamCityListener(options))
                yield return new TeamCityListener();
            else
                yield return new ConsoleListener();

            if (ShouldUseAppVeyorListener())
                yield return new AppVeyorListener();
        }

        static IEnumerable<Listener> ReportingListeners(Options options)
        {
            foreach (var fileName in options[CommandLineOption.NUnitXml])
                yield return new ReportListener<NUnitXmlReport>(fileName);

            foreach (var fileName in options[CommandLineOption.XUnitXml])
                yield return new ReportListener<XUnitXmlReport>(fileName);
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