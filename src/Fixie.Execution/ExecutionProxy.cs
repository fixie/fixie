namespace Fixie.Execution
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Listeners;

    public class ExecutionProxy : LongLivedMarshalByRefObject
    {
        public IReadOnlyList<MethodGroup> DiscoverTestMethodGroups(string assemblyFullPath, Options options)
        {
            var assembly = LoadAssembly(assemblyFullPath);

            return new Discoverer(options).DiscoverTestMethodGroups(assembly);
        }

        public AssemblyResult RunAssembly(string assemblyFullPath, Options options)
        {
            var assembly = LoadAssembly(assemblyFullPath);
            var listener = GetListener(options);

            return Runner(options, listener).RunAssembly(assembly);
        }

        public AssemblyResult RunAssembly<TListener>(string assemblyFullPath, Options options, object[] listenerArguments)
            where TListener : Listener
        {
            var assembly = LoadAssembly(assemblyFullPath);
            var listener = GetListener<TListener>(listenerArguments);

            return Runner(options, listener).RunAssembly(assembly);
        }

        public AssemblyResult RunMethods<TListener>(string assemblyFullPath, Options options, MethodGroup[] methodGroups, object[] listenerArguments)
            where TListener : Listener
        {
            var assembly = LoadAssembly(assemblyFullPath);
            var listener = GetListener<TListener>(listenerArguments);

            return Runner(options, listener).RunMethods(assembly, methodGroups);
        }

        static Assembly LoadAssembly(string assemblyFullPath)
        {
            return Assembly.Load(AssemblyName.GetAssemblyName(assemblyFullPath));
        }

        static Runner Runner(Options options, Listener listener)
        {
            var bus = new Bus(listener);
            return new Runner(bus, options);
        }

        static Listener GetListener<TListener>(object[] listenerArguments)
            where TListener : Listener
        {
            return (Listener)Activator.CreateInstance(typeof(TListener), listenerArguments);
        }

        static Listener GetListener(Options options)
        {
            if (ShouldUseTeamCityListener(options))
                return new TeamCityListener();

            if (ShouldUseAppVeyorListener())
                return new AppVeyorListener();

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

        static bool ShouldUseAppVeyorListener()
        {
            return Environment.GetEnvironmentVariable("APPVEYOR") == "True";
        }
    }
}