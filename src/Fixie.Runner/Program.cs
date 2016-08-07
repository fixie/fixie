namespace Fixie.Runner
{
    using System;
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

                var options = commandLineParser.Options;

                using (var environment = new ExecutionEnvironment(commandLineParser.AssemblyPath))
                {
                    if (ShouldUseTeamCityListener(options))
                        environment.Subscribe<TeamCityListener>();
                    else
                        environment.Subscribe<ConsoleListener>();

                    if (ShouldUseAppVeyorListener())
                        environment.Subscribe<AppVeyorListener>();

                    foreach (var format in options[CommandLineOption.ReportFormat])
                    {
                        if (String.Equals(format, "NUnit", StringComparison.CurrentCultureIgnoreCase))
                            environment.Subscribe<ReportListener<NUnitXml>>();

                        else if (String.Equals(format, "xUnit", StringComparison.CurrentCultureIgnoreCase))
                            environment.Subscribe<ReportListener<XUnitXml>>();
                    }

                    return environment.RunAssembly(options);
                }
            }
            catch (Exception exception)
            {
                using (Foreground.Red)
                    Console.WriteLine($"Fatal Error: {exception}");
                return FatalError;
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