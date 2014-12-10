using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fixie.Discovery;
using Fixie.Execution;
using Fixie.Listeners;
using Fixie.Results;

namespace Fixie
{
    public class ExecutionProxy : MarshalByRefObject
    {
        public AssemblyResult RunAssembly(string assemblyFullPath, string[] args, Listener listener)
        {
            var assembly = LoadAssembly(assemblyFullPath);

            return Runner(args, listener).RunAssembly(assembly);
        }

        public AssemblyResult RunMethods(string assemblyFullPath, string[] args, Listener listener, MethodGroup[] methodGroups)
        {
            var assembly = LoadAssembly(assemblyFullPath);

            var methods = GetMethods(methodGroups, assembly);

            return Runner(args, listener).RunMethods(assembly, methods);
        }

        static Assembly LoadAssembly(string assemblyFullPath)
        {
            return Assembly.Load(AssemblyName.GetAssemblyName(assemblyFullPath));
        }

        static Runner Runner(string[] args, Listener listener)
        {
            return new Runner(listener, new CommandLineParser(args).Options);
        }

        static MethodInfo[] GetMethods(MethodGroup[] methodGroups, Assembly assembly)
        {
            return methodGroups.SelectMany(x => GetMethodInfo(assembly, x)).ToArray();
        }

        static IEnumerable<MethodInfo> GetMethodInfo(Assembly assembly, MethodGroup methodGroup)
        {
            return assembly
                .GetType(methodGroup.Class)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.Name == methodGroup.Method);
        }

        [Obsolete("ConsoleListener and TeamCityListener should move into Fixie.Console.exe.")]
        public AssemblyResult RunAssembly(string assemblyFullPath, string[] args)
        {
            var listener = CreateListener(new CommandLineParser(args).Options);

            return RunAssembly(assemblyFullPath, args, listener);
        }

        [Obsolete("ConsoleListener and TeamCityListener should move into Fixie.Console.exe.")]
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