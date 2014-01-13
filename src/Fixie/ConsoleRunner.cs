using System;
using System.Collections.Generic;
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
            var listenerNames = options[CommandLineOption.Listener].ToArray();

            return listenerNames.Any()
                ? CreateCompoundListener(listenerNames)
                : CreateDefaultListener();
        }

        static Listener CreateCompoundListener(IEnumerable<string> listenerNames)
        {
            var compoundListener = new CompoundListener();

            foreach (var listenerName in listenerNames)
            {
                var listenerType = Assembly.GetExecutingAssembly().GetType(listenerName);
                var listener = (Listener)Activator.CreateInstance(listenerType);
                compoundListener.Add(listener);
            }

            return compoundListener;
        }

        static Listener CreateDefaultListener()
        {
            var runningUnderTeamCity = Environment.GetEnvironmentVariable("TEAMCITY_PROJECT_NAME") != null;

            if (runningUnderTeamCity)
                return new TeamCityListener();

            return new ConsoleListener();
        }
    }
}
