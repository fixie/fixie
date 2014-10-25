using System;
using System.Linq;
using System.Reflection;
using Fixie.Execution;
using Fixie.Listeners;
using Fixie.Results;

namespace Fixie
{
    [Obsolete("Refactor to use ExecutionProxy instead.")]
    public class ConsoleRunner : MarshalByRefObject
    {
        public AssemblyResult RunAssembly(string assemblyFullPath, string[] args)
        {
            var assembly = Assembly.Load(AssemblyName.GetAssemblyName(assemblyFullPath));

            var options = new CommandLineParser(args).Options;

            var runner = new Runner(CreateListener(options), options);
            return runner.RunAssembly(assembly);
        }

        static Listener CreateListener(ILookup<string, string> options)
        {
            var teamCityExplicitlySpecified = options.Contains(CommandLineOption.TeamCity);

            var runningUnderTeamCity = Environment.GetEnvironmentVariable("TEAMCITY_PROJECT_NAME") != null;

            var useTeamCityListener =
                (teamCityExplicitlySpecified && options[CommandLineOption.TeamCity].First() == "on") ||
                (!teamCityExplicitlySpecified && runningUnderTeamCity);

            if (useTeamCityListener)
                return new TeamCityListener();

            return new ConsoleListener();
        }
    }
}