using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Fixie.Listeners;

namespace Fixie
{
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
            var runningUnderTeamCity = Environment.GetEnvironmentVariable("TEAMCITY_PROJECT_NAME") != null;

            if (runningUnderTeamCity)
                return new TeamCityListener();
            else if (options.Contains ("nunit2"))
                return new NUnit2XmlOutputListener (new StreamWriter(options["nunit2"].FirstOrDefault()));

            return new ConsoleListener();
        }
    }
}