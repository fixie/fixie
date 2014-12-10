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
        public AssemblyResult RunAssembly(string assemblyFullPath, string[] args, Listener listener)
        {
            var assembly = LoadAssembly(assemblyFullPath);

            return Runner(args, listener).RunAssembly(assembly);
        }

        static Assembly LoadAssembly(string assemblyFullPath)
        {
            return Assembly.Load(AssemblyName.GetAssemblyName(assemblyFullPath));
        }

        static Runner Runner(string[] args, Listener listener)
        {
            return new Runner(listener, new CommandLineParser(args).Options);
        }

        public AssemblyResult RunAssembly(string assemblyFullPath, string[] args)
        {
            var listener = CreateListener(new CommandLineParser(args).Options);

            return RunAssembly(assemblyFullPath, args, listener);
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