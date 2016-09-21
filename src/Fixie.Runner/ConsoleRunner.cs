namespace Fixie.Runner
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Execution;
    using Reports;

    public class ConsoleRunner
    {
        public static int RunAssembly(Options options, IReadOnlyList<string> conventionArguments, ExecutionEnvironment environment)
        {
            if (ShouldUseTeamCityListener(options))
                environment.Subscribe<TeamCityListener>();
            else
                environment.Subscribe<ConsoleListener>();

            if (ShouldUseAppVeyorListener())
                environment.Subscribe<AppVeyorListener>();

            if (options.ReportFormat == ReportFormat.NUnit)
                environment.Subscribe<ReportListener<NUnitXml>>();
            else if (options.ReportFormat == ReportFormat.xUnit)
                environment.Subscribe<ReportListener<XUnitXml>>();

            return environment.RunAssembly(conventionArguments);
        }

        static bool ShouldUseTeamCityListener(Options options)
        {
            var teamCityExplicitlyEnabled = options.TeamCity == true;

            var runningUnderTeamCity = Environment.GetEnvironmentVariable("TEAMCITY_PROJECT_NAME") != null;

            return teamCityExplicitlyEnabled || runningUnderTeamCity;
        }

        static bool ShouldUseAppVeyorListener()
        {
            return Environment.GetEnvironmentVariable("APPVEYOR") == "True";
        }
    }
}