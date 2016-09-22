namespace Fixie.Runner
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Execution;
    using Execution.Listeners;
    using Reports;

    public class ConsoleRunner : RunnerBase
    {
        public override int Run(string assemblyFullPath, Assembly assembly, Options options, IReadOnlyList<string> conventionArguments)
        {
            var listeners = new List<Listener>();

            if (ShouldUseTeamCityListener(options))
                listeners.Add(new TeamCityListener());
            else
                listeners.Add(new ConsoleListener());

            if (ShouldUseAppVeyorListener())
                listeners.Add(new AppVeyorListener());

            if (options.ReportFormat == ReportFormat.NUnit)
                listeners.Add(new ReportListener<NUnitXml>());
            else if (options.ReportFormat == ReportFormat.xUnit)
                listeners.Add(new ReportListener<XUnitXml>());

            var summaryListener = new SummaryListener();
            listeners.Add(summaryListener);

            RunAssembly(assembly, conventionArguments, listeners);

            return summaryListener.Summary.Failed;
        }

        static bool ShouldUseTeamCityListener(Options options)
        {
            var teamCityExplicitlyEnabled = options.TeamCity == true;

            var runningUnderTeamCity = Environment.GetEnvironmentVariable("TEAMCITY_PROJECT_NAME") != null;

            return teamCityExplicitlyEnabled || runningUnderTeamCity;
        }

        static bool ShouldUseAppVeyorListener()
            => Environment.GetEnvironmentVariable("APPVEYOR") == "True";
    }
}