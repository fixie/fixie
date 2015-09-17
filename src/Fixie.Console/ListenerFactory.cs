using System;
using System.Linq;
using Fixie.Execution;

namespace Fixie.ConsoleRunner
{
    public class ListenerFactory : IListenerFactory
    {
        public Listener Create(Options options, IExecutionSink executionSink)
        {
            if (ShouldUseTeamCityListener(options))
                return new TeamCityListener();

            return new ConsoleListener();
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
    }
}