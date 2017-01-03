namespace Fixie.Execution
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Internal;
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
            var listener = GetListener(options);
            var assembly = LoadAssembly(assemblyFullPath);

            using (var bus = new Bus(listener))
                return Runner(options, bus).RunAssembly(assembly);
        }

        public AssemblyResult RunAssembly(string assemblyFullPath, Options options, Bus bus)
        {
            var assembly = LoadAssembly(assemblyFullPath);

            return Runner(options, bus).RunAssembly(assembly);
        }

        public AssemblyResult RunMethods(string assemblyFullPath, Options options, Bus bus, MethodGroup[] methodGroups)
        {
            var assembly = LoadAssembly(assemblyFullPath);

            return Runner(options, bus).RunMethods(assembly, methodGroups);
        }

        static Assembly LoadAssembly(string assemblyFullPath)
        {
            return Assembly.Load(AssemblyName.GetAssemblyName(assemblyFullPath));
        }

        static Runner Runner(Options options, Bus bus)
        {
            return new Runner(bus, options);
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