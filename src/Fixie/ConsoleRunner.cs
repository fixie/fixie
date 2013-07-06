using System;
using System.IO;
using System.Reflection;

namespace Fixie
{
    public class ConsoleRunner : MarshalByRefObject
    {
        public Result RunAssembly(string assemblyPath, string[] args)
        {
            var assemblyFullPath = Path.GetFullPath(assemblyPath);
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