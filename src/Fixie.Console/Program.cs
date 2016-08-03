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

            var listeners = Listeners(options).ToList();

            listeners.Add(summaryListener);

            using (var environment = new ExecutionEnvironment(commandLineParser.AssemblyPath))
                environment.RunAssembly(listeners, options);

            return summaryListener.Summary;
        }

        static IEnumerable<Listener> Listeners(Options options)
        {
            if (ShouldUseTeamCityListener(options))
                yield return new TeamCityListener();
            else
                yield return new ConsoleListener();

            if (ShouldUseAppVeyorListener())
                yield return new AppVeyorListener();

            foreach (var format in options[CommandLineOption.ReportFormat])
            {
                if (String.Equals(format, "NUnit", StringComparison.CurrentCultureIgnoreCase))
                    yield return new ReportListener<NUnitXml>();

                else if (String.Equals(format, "xUnit", StringComparison.CurrentCultureIgnoreCase))
                    yield return new ReportListener<XUnitXml>();
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