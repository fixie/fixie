using System;
using System.Reflection;
using Fixie.Listeners;

namespace Fixie
{
    public class ConsoleRunner : MarshalByRefObject
    {
        public Result RunAssembly(string assemblyFullPath, string[] args)
        {
            var assembly = Assembly.Load(AssemblyName.GetAssemblyName(assemblyFullPath));

            var options = new CommandLineParser(args).Options;

            var runner = new Runner(CreateListener(), options);
            return runner.RunAssembly(assembly);
        }

        static Listener CreateListener()
        {
            var runningUnderTeamCity = Environment.GetEnvironmentVariable("TEAMCITY_PROJECT_NAME") != null;

            if (runningUnderTeamCity)
                return new TeamCityListener();

            return new ConsoleListener();
        }
    }
}